using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using LinqKit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Claims;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _repository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="mapper">The mapper.</param>
        public TicketService(ITicketRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        #region Ticket CRUD Operations
        /// <summary>
        /// Add a new ticket
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        public void Add(TicketViewModel ticket, string userId)
        {
            HandleAttachment(ticket);

            var newTicket = _mapper.Map<Ticket>(ticket);
            newTicket.CreatedDate = DateTime.Now;
            newTicket.UpdatedDate = null;
            newTicket.ResolvedDate = null;
            newTicket.UserId = userId;

            AssignTicketProperties(newTicket);

            string id = _repository.Add(newTicket);
            if (ticket.File != null)
            {
                ticket.Attachment.TicketId = id;
                AddAttachment(ticket.Attachment);
            }
        }

        /// <summary>
        /// Update an existing ticket
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        public void Update(TicketViewModel ticket)
        {
            HandleAttachment(ticket);

            var existingTicket = _repository.FindById(ticket.TicketId);
            if (existingTicket == null)
            {
                throw new ArgumentException($"Ticket with ID {ticket.TicketId} not found.");
            }

            ticket.UpdatedDate = DateTime.Now; // TODO: only update if ticket properties have changed

            _mapper.Map(ticket, existingTicket);
            UpdateTicketResolvedDate(existingTicket);
            SetNavigationProperties(existingTicket);

            string id = _repository.Update(existingTicket);
            if (ticket.File != null)
            {
                ticket.Attachment.TicketId = id;
                AddAttachment(ticket.Attachment);
            }
        }

        /// <summary>
        /// Delete a ticket
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void Delete(string id)
        {
            var ticket = _repository.FindById(id);
            if (ticket != null)
            {
                RemoveAttachmentByTicketId(ticket.TicketId);
                RemoveAssignmentByTicketId(ticket.TicketId);
                _repository.Delete(ticket);
            }
        }
        #endregion Ticket CRUD Operations

        #region Ticket Attachment CRUD Operations
        /// <summary>
        /// Add a new attachment
        /// </summary>
        /// <param name="ticket">The attachment.</param>
        public void AddAttachment(Attachment attachment)
        {
            attachment.Ticket = _repository.FindById(attachment.TicketId);
            _repository.AddAttachment(attachment);
        }

        /// <summary>
        /// Remove a ticket attachment.
        /// </summary>
        /// <param name="attachmentId">The attachment identifier.</param>
        public void RemoveAttachment(string attachmentId)
        {
            var attachment = _repository.FindAttachmentById(attachmentId);
            if (attachment != null) _repository.RemoveAttachment(attachment);
        }
        #endregion Ticket Attachment CRUD Operations

        #region Ticket Assignment CRUD Operations
        /// <summary>
        /// Assigns the ticket to agent.
        /// </summary>
        /// <param name="model">The model.</param>
        public void AddTicketAssignment(TicketViewModel model)
        {
            var assignment = GetAssignmentByTicketId(model.TicketId);
            if (assignment == null)
            {
                assignment = CreateTicketAssignment(model);
                _repository.AssignTicket(assignment);
            }
        }

        /// <summary>
        /// Removes a ticket assignment.
        /// </summary>
        /// <param name="id">The ticket identifier.</param>
        public void RemoveAssignment(string id)
        {
            var assignment = _repository.FindAssignmentByTicketId(id);
            if (assignment != null) _repository.RemoveAssignment(assignment);
        }
        #endregion Ticket Assignment CRUD Operations

        #region Get Methods
        /// <summary>
        /// Calls the repository to get all tickets.
        /// Maps the list of Ticket to a list of TicketViewModel.
        /// </summary>
        /// <returns>A list of TicketViewModel (IEnumerable)</returns>
        public IEnumerable<TicketViewModel> GetAll()
        {
            var tickets = _repository.GetAll().ToList();
            tickets.ForEach(ticket => SetNavigationProperties(ticket));

            var ticketViewModels = _mapper.Map<IEnumerable<TicketViewModel>>(tickets).ToList();
            ticketViewModels.ForEach(ticket => ticket.Attachment = GetAttachmentByTicketId(ticket.TicketId));

            return ticketViewModels;
        }

        /// <summary>
        /// Calls the repository to get a ticket by its id.
        /// Maps the Ticket to the TicketViewModel.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>TicketViewModel</returns>
        public TicketViewModel GetTicketById(string id)
        {
            var ticket = _repository.FindById(id);
            SetNavigationProperties(ticket);

            var mappedTicket = _mapper.Map<TicketViewModel>(ticket);
            mappedTicket.Attachment = GetAttachmentByTicketId(id);
            mappedTicket.TicketAssignment = GetAssignmentByTicketId(id);
            if (mappedTicket.TicketAssignment != null && !string.IsNullOrEmpty(mappedTicket.TicketAssignment.AssignmentId))
            {
                mappedTicket.Agent = GetAgentById(ExtractAgentId(mappedTicket.TicketAssignment.AssignmentId));
            }

            return mappedTicket;
        }

        /// <summary>
        /// Calls the repository to get unresolved tickets (Open, In Progress).
        /// </summary>
        /// <returns>A list of TicketViewModel (IEnumerable)</returns>
        public IEnumerable<TicketViewModel> GetUnresolvedTickets()
        {
            var tickets = GetAll()
                          .Where(ticket => ticket.StatusType.StatusName == "Open" || ticket.StatusType.StatusName == "In Progress")
                          .ToList();
            return _mapper.Map<IEnumerable<TicketViewModel>>(tickets);
        }

        /// <summary>
        /// Gets the tickets by assignment status.
        /// </summary>
        /// <param name="status">The status, "assigned" or "unassigned"</param>
        /// <returns>A list of TicketViewModel (IEnumerable)</returns>
        public IEnumerable<TicketViewModel> GetTicketsByAssignmentStatus(string status)
        {
            var assignedTicketIds = GetTicketAssignments().Select(ta => ta.TicketId).ToList();
            var tickets = _repository.GetTickets(status, assignedTicketIds).ToList();
            tickets.ForEach(ticket => SetNavigationProperties(ticket));

            return _mapper.Map<IEnumerable<TicketViewModel>>(tickets);
        }

        /// <summary>
        /// Retrieves ticket details for a specific ticket.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Json</returns>
        public Object GetTicketDetails(string id)
        {
            var ticket = GetTicketById(id);
            if (ticket != null)
            {
                return new
                {
                    ticket.TicketId,
                    ticket.Subject,
                    ticket.IssueDescription,
                    ticket.StatusTypeId,
                    ticket.CategoryTypeId,
                    ticket.PriorityTypeId,
                    ticket.Agent
                };
            }
            return null;
        }

        /// <summary>
        /// Calls the repository to get an attachment by its ticket id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Attachment</returns>
        public Attachment GetAttachmentByTicketId(string id) => _repository.FindAttachmentByTicketId(id);

        /// <summary>
        /// Calls the repository to get an assignment by its ticket id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>TicketAssignment</returns>
        public TicketAssignment GetAssignmentByTicketId(string id) => _repository.FindAssignmentByTicketId(id);

        /// <summary>
        /// Calls the repository to get a Team through TeamMember userId.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Team</returns>
        public Team GetTeamByUserId(string id) => _repository.FindTeamByUserId(id);

        /// <summary>
        /// Calls the repository to get an agent by its user id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A User with "Support Agent" role</returns>
        public User GetAgentById(string id) => _repository.FindAgentByUserId(id);

        /// <summary>
        /// Calls the repository to get all category types.
        /// </summary>
        /// <returns>A list of CategoryType (IEnumerable)</returns>
        public IEnumerable<CategoryType> GetCategoryTypes() => _repository.GetCategoryTypes();

        /// <summary>
        /// Calls the repository to get all priority types.
        /// </summary>
        /// <returns>A list of PriorityType (IEnumerable)</returns>
        public IEnumerable<PriorityType> GetPriorityTypes() => _repository.GetPriorityTypes();

        /// <summary>
        /// Calls the repository to get all status types.
        /// </summary>
        /// <returns>A list of StatusType (IEnumerable)</returns>
        public IEnumerable<StatusType> GetStatusTypes() => _repository.GetStatusTypes();

        /// <summary>
        /// Calls the repository to get all support agents.
        /// </summary>
        /// <returns>A list of User (IEnumerable) with "Support Agent" role</returns>
        public IEnumerable<User> GetSupportAgents() => _repository.GetSupportAgents();

        /// <summary>
        /// Calls the repository to get all ticket assignments.
        /// </summary>
        /// <returns>A list of TicketAssignment (IEnumerable)</returns>
        public IEnumerable<TicketAssignment> GetTicketAssignments() => _repository.GetTicketAssignments();
        #endregion Get Methods

        #region Utility Methods
        /// <summary>
        /// Initializes a TicketViewModel to display.
        /// </summary>
        /// <returns>TicketViewModel</returns>
        public TicketViewModel InitializeModel(string type)
        {
            var tickets = type switch
            {
                "status" => GetAll(),
                "assign" => GetTicketsByAssignmentStatus("unassigned"),
                "assigned" => GetTicketsByAssignmentStatus("reassign"),
                _ => GetUnresolvedTickets(),
            };

            return new TicketViewModel
            {
                Tickets = tickets,
                CategoryTypes = GetCategoryTypes(),
                PriorityTypes = GetPriorityTypes(),
                StatusTypes = GetStatusTypes(),
                Agents = GetSupportAgents()
            };
        }

        /// <summary>
        /// Extracts the agent identifier from assignment identifier.
        /// </summary>
        /// <param name="assignmentId">The assignment identifier.</param>
        /// <returns>Support agent identifier string</returns>
        public string ExtractAgentId(string assignmentId)
        {
            if (assignmentId == null)
                return string.Empty;

            string[] parts = assignmentId.Split('-');

            if (parts.Length >= 5)
            {
                var str = string.Join("-", parts.Take(5));
                return str;
            }
            else
            {
                return assignmentId;
            }
        }

        /// <summary>
        /// Handles the attachment by converting the File to an Attachment 
        /// and assigning it to the ticket.
        /// </summary>
        /// <param name="ticket">The ticket</param>
        private void HandleAttachment(TicketViewModel ticket)
        {
            if (ticket.File != null && ticket.File.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    ticket.File.CopyTo(stream);
                    ticket.Attachment = new Attachment
                    {
                        AttachmentId = Guid.NewGuid().ToString(),
                        Name = ticket.File.FileName,
                        Content = stream.ToArray(),
                        Type = ticket.File.ContentType,
                        UploadedDate = DateTime.Now
                    };
                }
            }
        }

        /// <summary>
        /// Updates the ticket resolved date based on the status.
        /// </summary>
        /// <param name="ticket"></param>
        /// <exception cref="ArgumentException"></exception>
        private void UpdateTicketResolvedDate(Ticket ticket)
        {
            var status = _repository.FindStatusById(ticket.StatusTypeId);

            if (status != null && (status.StatusName.Equals("Closed") || status.StatusName.Equals("Resolved")))
            {
                ticket.ResolvedDate = ticket.ResolvedDate ?? DateTime.Now;
            }
            else
            {
                ticket.ResolvedDate = null;
            }
        }

        /// <summary>
        /// Assigns the following ticket properties: TicketId, CategoryType, PriorityType, StatusType
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        /// 
        /// Ticket ID format: CC-CN-NN
        /// CC = Category Type ID
        /// CN = Category Number (how many tickets have been created for a category)
        /// NN = Ticket Number (total number of tickets created)
        private void AssignTicketProperties(Ticket ticket)
        {
            /// TicketId is reused if an entry is deleted and a new one has the same category as deleted entry
            /// not scalable if database has many entries
            // TODO: revisit this logic
            int CC = Convert.ToInt32(ticket.CategoryTypeId);
            int CN = GetAll().Count(t => t.CategoryTypeId == ticket.CategoryTypeId);
            int NN = GetAll().Count();

            ticket.TicketId = $"{CC:00}-{CN + 1:00}-{NN + 1:00}";

            ticket.StatusTypeId ??= "1";
            SetNavigationProperties(ticket);
        }

        /// <summary>
        /// Sets the navigation properties for a ticket: CategoryType, PriorityType, StatusType
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        private void SetNavigationProperties(Ticket ticket)
        {
            if (ticket == null) return;

            ticket.CategoryType = GetCategoryTypes().Single(x => x.CategoryTypeId == ticket.CategoryTypeId);
            ticket.PriorityType = GetPriorityTypes().Single(x => x.PriorityTypeId == ticket.PriorityTypeId);
            ticket.StatusType = GetStatusTypes().Single(x => x.StatusTypeId == ticket.StatusTypeId);
            // TODO: add User navigation property
        }

        /// <summary>
        /// Creates a new ticket assignment.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>TicketAssignment</returns>
        /// AssignmentId format: {AgentId}-{Timestamp}-{RandomNumber}
        private TicketAssignment CreateTicketAssignment(TicketViewModel model)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var randomNumber = new Random().Next(1000, 9999);
            var assignmentId = $"{model.Agent.UserId}-{timestamp}-{randomNumber}";

            return new TicketAssignment
            {
                AssignmentId = assignmentId,
                TeamId = GetTeamByUserId(model.Agent.UserId).TeamId,
                TicketId = model.TicketId,
                AssignedDate = DateTime.Now,
                AdminId = "D56F556E-50A4-4240-A0FF-9A6898B3A03B" // Hardcoded due to lack of admin login
            };
        }

        /// <summary>
        /// Removes the attachment by ticket identifier.
        /// </summary>
        /// <param name="id">Ticket identifier</param>
        private void RemoveAttachmentByTicketId(string id)
        {
            var attachment = GetAttachmentByTicketId(id);
            if (attachment != null) _repository.RemoveAttachment(attachment);
        }

        /// <summary>
        /// Removes the assignment by ticket identifier.
        /// </summary>
        /// <param name="id">Ticket identifier</param>
        private void RemoveAssignmentByTicketId(string id)
        {
            var assignment = GetAssignmentByTicketId(id);
            if (assignment != null) _repository.RemoveAssignment(assignment);
        }
        #endregion Utility Methods
    }
}