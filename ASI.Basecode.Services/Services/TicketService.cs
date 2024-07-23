using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using ZXing;
using static ASI.Basecode.Services.Exceptions.TicketExceptions;
using static ASI.Basecode.Services.Exceptions.TeamExceptions;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public partial class TicketService : ITicketService
    {
        private readonly ITicketRepository _repository;
        private readonly IUserPreferencesRepository _userPreferencesRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotificationService _notificationService;
        private readonly ITeamRepository _teamRepository;
        private readonly IPerformanceReportRepository _performanceReportRepository;
        private readonly IActivityLogService _activityLogService;
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketService"/> class.
        /// </summary>
        /// <param name="repository">The repository</param>
        /// <param name="mapper">The mapper</param>
        /// <param name="logger">The logger</param>
        /// <param name="httpContextAccessor">The HTTP context accessor</param>
        public TicketService(
            ITicketRepository repository,
            IUserPreferencesRepository userPreferencesRepository,
            IMapper mapper,
            INotificationService notificationService,
            IHttpContextAccessor httpContextAccessor,
            IPerformanceReportRepository performanceReportRepository,
            ITeamRepository teamRepository,
            IActivityLogService activityLogService,
            IUserRepository userRepository)
        {
            _repository = repository;
            _userPreferencesRepository = userPreferencesRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _notificationService = notificationService;
            _performanceReportRepository = performanceReportRepository;
            _teamRepository = teamRepository;
            _activityLogService = activityLogService;
            _userRepository = userRepository;
        }

        #region Ticket CRUD Operations
        /// <summary>
        /// Calls the repository to add a new ticket.
        /// </summary>
        /// <param name="model">Ticket identifier</param>
        /// <param name="userId">User identifier</param>
        public async Task AddAsync(TicketViewModel model, string userId)
        {
            if (await IsDuplicateTicketAsync(model, userId))
                throw new TicketException("A similar ticket already exists.");

            if (model != null)
            {
                if(model.Subject.Length > 100)
                    throw new TicketException("Subject has exceeded maximum allowed characters of 100.");
                if (model.IssueDescription.Length > 800)
                    throw new TicketException("Description has exceeded maximum allowed characters of 800.");

                if (model.File != null)
                    await HandleAttachmentAsync(model);

                var newTicket = _mapper.Map<Ticket>(model);
                newTicket.CreatedDate = DateTime.Now;
                newTicket.UpdatedDate = DateTime.Now;
                newTicket.UserId = userId;

                AssignTicketProperties(newTicket);

                if (model.File != null && model.Attachment.AttachmentId != null)
                {
                    model.Attachment.TicketId = newTicket.TicketId;
                    await AddAttachmentAsync(model.Attachment, newTicket);
                }
                else
                {
                    await _repository.AddAsync(newTicket);
                }
                _notificationService.CreateNotification(newTicket, 1, null);

                // Log the creation activity
                await _activityLogService.LogActivityAsync(newTicket, userId, "Create", $"Ticket created");
            }
        }

        /// <summary>
        /// Calls the repository to update an existing ticket.
        /// </summary>
        /// <param name="model">The ticket</param>
        public async Task UpdateAsync(TicketViewModel model)
        {
            var ticket = await _repository.FindByIdAsync(model.TicketId);
            if (await IsDuplicateTicketAsync(model, model.UserId) && ticket.Subject != model.Subject)
                throw new TicketException("A similar ticket already exists.");

            if (ticket != null)
            {
                if (!string.IsNullOrEmpty(model.CategoryTypeId))
                {
                    if(!string.IsNullOrEmpty(model.CategoryTypeId) && ticket.CategoryTypeId == model.CategoryTypeId)
                        throw new TicketException("Ticket is already in the selected category.");

                    ticket.CategoryTypeId = model.CategoryTypeId;
                    await _repository.UpdateAsync(ticket);
                    return;
                }

                if (model.Subject.Length > 100)
                    throw new TicketException("Subject has exceeded maximum allowed characters of 100.");
                if (model.IssueDescription.Length > 800)
                    throw new TicketException("Description has exceeded maximum allowed characters of 800.");

                bool hasChanges = ticket.IssueDescription != model.IssueDescription ||
                                ticket.Subject != model.Subject ||
                                (!ticket.Attachments.Any() && model.File != null);
                bool hasAttachmentChanges = ticket.Attachments.Any() && (model.File != null || 
                                (model.File == null && model.Attachment == null));

                if ((hasChanges || hasAttachmentChanges) && ticket.ResolvedDate != null)
                    throw new TicketException("Cannot make changes to resolved tickets.", model.TicketId);
                if (!hasChanges && !hasAttachmentChanges)
                    throw new TicketException("No changes were made to the ticket.", model.TicketId);

                if (hasAttachmentChanges)
                {
                    await RemoveAttachmentByTicketIdAsync(ticket.TicketId);
                    ticket.Attachments.Clear();
                }
                if (model.File != null) await HandleAttachmentAsync(model);

                ticket.Subject = model.Subject != null ? model.Subject : ticket.Subject;
                ticket.IssueDescription = model.IssueDescription != null ? model.IssueDescription : ticket.IssueDescription;
                if(model.File != null) ticket.Attachments.Add(model.Attachment);

                ticket.UpdatedDate = DateTime.Now;
                await UpdateTicketDate(ticket);
                
                if (model.File != null && model.Attachment.AttachmentId != null)
                {
                    model.Attachment.TicketId = ticket.TicketId;
                    await AddAttachmentAsync(model.Attachment, ticket);
                }
                else
                {
                    await _repository.UpdateAsync(ticket);
                }
                if (hasChanges || hasAttachmentChanges)
                {
                    await _activityLogService.LogActivityAsync(ticket, _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value, "Ticket Update", 
                        $"{(hasChanges ? "Details" : "")}{(hasChanges && hasAttachmentChanges ? " & " : "")}{(hasAttachmentChanges ? "Attachment" : "")} modified");
                    _notificationService.CreateNotification(ticket, 4, null, ticket.TicketAssignment?.AgentId);
                }
            }
            else
            {
                throw new TicketException("Ticket does not exist.");
            }
        }

        /// <summary>
        /// Calls the repository to delete a ticket.
        /// Deletes all associated data (attachments, assignments, feedback, notifications, and comments).
        /// </summary>
        /// <param name="id">Ticket identifier</param>
        public async Task DeleteAsync(string id)
        {
            var ticket = await _repository.FindByIdAsync(id);
            if (ticket != null)
            {
                await _repository.DeleteAsync(ticket);

                await _activityLogService.LogActivityAsync(ticket, _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value, "Delete", $"Ticket deleted");
            }
            else throw new TicketException("Ticket does not exist.");

        }
        #endregion Ticket CRUD Operations

        #region Utility Methods
        /// <summary>
        /// Calls the repository to check if a ticket is a duplicate.
        /// </summary>
        /// <param name="ticket">The new ticket</param>
        /// <param name="userId">User identifier</param>
        /// <returns>true or false</returns>
        private async Task<bool> IsDuplicateTicketAsync(TicketViewModel ticket, string userId)
        {
            var duplicateTickets = await _repository.FindByUserIdAsync(userId);
            return duplicateTickets.Any(t => t.Subject.ToLower() == ticket.Subject.ToLower() &&
                                             t.CategoryTypeId == ticket.CategoryTypeId);
        }

        /// <summary>
        /// Helper method to assign ticket properties.
        /// </summary>
        /// <param name="ticket">The ticket</param>
        private void AssignTicketProperties(Ticket ticket)
        {
            string CC = ticket.CategoryTypeId;
            int NN = _repository.CountAllAndDeletedTicketsAsync().Result;

            ticket.TicketId = $"{CC}-{NN + 1:0000}";

            ticket.StatusTypeId ??= "S1";
            ticket.CategoryType = GetCategoryTypesAsync().Result.Single(x => x.CategoryTypeId == ticket.CategoryTypeId);
            ticket.PriorityType = GetPriorityTypesAsync().Result.Single(x => x.PriorityTypeId == ticket.PriorityTypeId);
            ticket.StatusType = GetStatusTypesAsync().Result.Single(x => x.StatusTypeId == ticket.StatusTypeId);
        }

        /// <summary>
        /// Helper method to update the ticket resolved date based on status.
        /// </summary>
        /// <param name="ticket">The ticket</param>
        private async Task UpdateTicketDate(Ticket ticket)
        {
            var status = await _repository.FindStatusByIdAsync(ticket.StatusTypeId);

            if (status != null && (status.StatusName.Equals("Closed") || status.StatusName.Equals("Resolved")))
            {
                ticket.ResolvedDate = ticket.ResolvedDate ?? DateTime.Now;
            }
            else
            {
                ticket.ResolvedDate = null;
            }
        }
        #endregion Utility Methods
    }
}
