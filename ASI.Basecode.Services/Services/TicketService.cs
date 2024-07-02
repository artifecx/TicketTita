using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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

        /// <summary>
        /// Calls the repository to add a new ticket
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        public string Add(TicketViewModel ticket, string userId)
        {
            var newTicket = _mapper.Map<Ticket>(ticket);
            newTicket.CreatedDate = DateTime.Now;
            newTicket.UpdatedDate = null;
            newTicket.ResolvedDate = null;
            newTicket.UserId = userId;

            return _repository.Add(newTicket);
        }

        /// <summary>
        /// Calls the repository to update an existing ticket
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        public string Update(TicketViewModel ticket)
        {
            var existingTicket = _repository.FindById(ticket.TicketId);
            if (existingTicket == null)
            {
                throw new ArgumentException($"Ticket with ID {ticket.TicketId} not found.");
            }

            UpdateTicketStatus(ticket);
            ticket.UpdatedDate = DateTime.Now;

            _mapper.Map(ticket, existingTicket);
            return _repository.Update(existingTicket);
        }

        /// <summary>
        /// Calls the repository to delete a ticket
        /// </summary>
        /// <param name="id">The identifier.</param>
        public void Delete(string id)
        {
            var ticket = _repository.FindById(id);
            if(ticket != null) _repository.Delete(ticket);
        }

        /// <summary>
        /// Calls the repository to add a new attachment
        /// </summary>
        /// <param name="ticket">The attachment.</param>
        public void AddAttachment(Attachment attachment)
        {
            if(attachment != null) _repository.AddAttachment(attachment);
        }

        /// <summary>
        /// Removes the ticket attachment.
        /// </summary>
        /// <param name="attachmentId">The attachment identifier.</param>
        public void RemoveAttachment(string attachmentId)
        {
            var attachment = _repository.FindAttachmentById(attachmentId);
            if (attachment != null) _repository.RemoveAttachment(attachment);
        }

        public void AssignTicket(TicketAssignment assignment)
        {
            if(assignment != null) _repository.AssignTicket(assignment);
        }

        public void RemoveAssignment(string id)
        {
            var assignment = _repository.FindAssignmentByTicketId(id);
            if (assignment != null) _repository.RemoveAssignment(assignment);
        }

        #region Get Methods        
        /// <summary>
        /// Calls the repository to get a ticket by its id.
        /// Maps the Ticket to the TicketViewModel.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>TicketViewModel</returns>
        public TicketViewModel GetTicketById(string id)
        {
            var ticket = _repository.FindById(id);
            return _mapper.Map<TicketViewModel>(ticket);
        }

        /// <summary>
        /// Calls the repository to get an attachment by its ticket id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Attachment</returns>
        public Attachment GetAttachmentByTicketId(string id)
        {
            return _repository.FindAttachmentByTicketId(id);
        }

        public TicketAssignment GetAssignmentByTicketId(string id)
        {
            return _repository.FindAssignmentByTicketId(id);
        }

        public Team GetTeamByUserId(string id)
        {
            return _repository.FindTeamByUserId(id);
        }

        public User GetAgentById(string id)
        {
            return _repository.FindAgentByUserId(id);
        }

        /// <summary>
        /// Calls the repository to get all tickets.
        /// Maps the list of Ticket to a list of TicketViewModel.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TicketViewModel> GetAll()
        {
            var tickets = _repository.GetAll();
            return _mapper.Map<IEnumerable<TicketViewModel>>(tickets);
        }

        public IEnumerable<TicketViewModel> GetTickets(string type)
        {
            var unassignedTickets = _repository.GetTickets(type);
            return _mapper.Map<IEnumerable<TicketViewModel>>(unassignedTickets);
        }

        /// <summary>
        /// Calls the repository to get all category types.
        /// </summary>
        /// <returns>a list of CategoryType (IEnumerable)</returns>
        public IEnumerable<CategoryType> GetCategoryTypes()
        {
            return _repository.GetCategoryTypes();
        }

        /// <summary>
        /// Calls the repository to get all priority types.
        /// </summary>
        /// <returns>a list of PriorityType (IEnumerable)</returns>
        public IEnumerable<PriorityType> GetPriorityTypes()
        {
            return _repository.GetPriorityTypes();
        }

        /// <summary>
        /// Calls the repository to get all status types.
        /// </summary>
        /// <returns>a list of StatusType (IEnumerable)</returns>
        public IEnumerable<StatusType> GetStatusTypes()
        {
            return _repository.GetStatusTypes();
        }

        public IEnumerable<User> GetSupportAgents()
        {
            return _repository.GetSupportAgents();
        }

        private void UpdateTicketStatus(TicketViewModel ticket)
        {
            var status = _repository.FindStatusById(ticket.StatusTypeId);

            if (status == null)
            {
                throw new ArgumentException($"Status with ID {ticket.StatusTypeId} not found.");
            }

            if (status.StatusName.Equals("Closed") && ticket.ResolvedDate == null)
            {
                ticket.ResolvedDate ??= DateTime.Now;
            }
            else if(!status.StatusName.Equals("Closed") && ticket.ResolvedDate != null)
            {
                ticket.ResolvedDate = null;
            }
        }
        #endregion
    }
}