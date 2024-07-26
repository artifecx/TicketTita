using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static ASI.Basecode.Services.Exceptions.TicketExceptions;
using ASI.Basecode.Resources.Messages;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Service class for handling operations related to tickets.
    /// </summary>
    public partial class TicketService : ITicketService
    {
        private readonly ITicketRepository _repository;
        private readonly IUserPreferencesRepository _userPreferencesRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotificationService _notificationService;
        private readonly ITeamRepository _teamRepository;
        private readonly IPerformanceReportService _performanceReportService;
        private readonly IActivityLogService _activityLogService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketService"/> class.
        /// </summary>
        /// <param name="repository">The ticket repository.</param>
        /// <param name="userPreferencesRepository">The user preferences repository.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="notificationService">The notification service.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="performanceReportService">The performance report service.</param>
        /// <param name="teamRepository">The team repository.</param>
        /// <param name="activityLogService">The activity log service.</param>
        public TicketService(
            ITicketRepository repository,
            IUserPreferencesRepository userPreferencesRepository,
            IMapper mapper,
            INotificationService notificationService,
            IHttpContextAccessor httpContextAccessor,
            IPerformanceReportService performanceReportService,
            ITeamRepository teamRepository,
            IActivityLogService activityLogService)
        {
            _repository = repository;
            _userPreferencesRepository = userPreferencesRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _notificationService = notificationService;
            _performanceReportService = performanceReportService;
            _teamRepository = teamRepository;
            _activityLogService = activityLogService;
        }

        #region Ticket CRUD Operations
        /// <summary>
        /// Adds a new ticket asynchronously.
        /// </summary>
        /// <param name="model">The ticket view model.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="TicketException">Thrown when a similar ticket already exists or the subject/description exceeds the allowed characters.</exception>
        public async Task AddAsync(TicketViewModel model, string userId)
        {
            if (await IsDuplicateTicketAsync(model, userId))
                throw new TicketException(Errors.DuplicateTicket);

            if (model != null)
            {
                if (model.Subject.Length > 100)
                    throw new TicketException(Errors.SubjectExceeded);
                if (model.IssueDescription.Length > 800)
                    throw new TicketException(Errors.DescriptionExceeded);

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
                await _activityLogService.LogActivityAsync(newTicket, userId, Common.TicketCreated, Common.TicketCreated);
            }
        }

        /// <summary>
        /// Updates an existing ticket asynchronously.
        /// </summary>
        /// <param name="model">The ticket view model.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="TicketException">Thrown when a similar ticket already exists, the subject/description exceeds the allowed characters, or changes are attempted on a resolved ticket.</exception>
        public async Task UpdateAsync(TicketViewModel model)
        {
            var ticket = await _repository.FindByIdAsync(model.TicketId);
            if (await IsDuplicateTicketAsync(model, model.UserId) && ticket.Subject != model.Subject)
                throw new TicketException(Errors.DuplicateTicket);

            if (ticket != null)
            {
                if (!string.IsNullOrEmpty(model.CategoryTypeId))
                {
                    if (!string.IsNullOrEmpty(model.CategoryTypeId) && ticket.CategoryTypeId == model.CategoryTypeId)
                        throw new TicketException(Errors.DuplicateTicketCategory);

                    ticket.CategoryTypeId = model.CategoryTypeId;
                    await _repository.UpdateAsync(ticket);
                    await _activityLogService.LogActivityAsync(ticket, _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value, "Ticket Update", Common.CategoryModified);
                    _notificationService.CreateNotification(ticket, 4, null, ticket.TicketAssignment?.AgentId);
                    return;
                }

                if (model.Subject.Length > 100)
                    throw new TicketException(Errors.SubjectExceeded);
                if (model.IssueDescription.Length > 800)
                    throw new TicketException(Errors.DescriptionExceeded);

                bool hasChanges = ticket.IssueDescription != model.IssueDescription ||
                                ticket.Subject != model.Subject ||
                                (!ticket.Attachments.Any() && model.File != null);
                bool hasAttachmentChanges = ticket.Attachments.Any() && (model.File != null ||
                                (model.File == null && model.Attachment == null));

                if ((hasChanges || hasAttachmentChanges) && ticket.ResolvedDate != null)
                    throw new TicketException(Errors.ResolvedTicketChanges, model.TicketId);
                if (!hasChanges && !hasAttachmentChanges)
                    throw new TicketException(Errors.NoChangesToTicket, model.TicketId);

                if (hasAttachmentChanges)
                {
                    await RemoveAttachmentByTicketIdAsync(ticket.TicketId);
                    ticket.Attachments.Clear();
                }
                if (model.File != null) await HandleAttachmentAsync(model);

                ticket.Subject = model.Subject != null ? model.Subject : ticket.Subject;
                ticket.IssueDescription = model.IssueDescription != null ? model.IssueDescription : ticket.IssueDescription;
                if (model.File != null) ticket.Attachments.Add(model.Attachment);

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
                    await _activityLogService.LogActivityAsync(ticket, _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value, Common.TicketUpdate,
                        $"{(hasChanges ? Common.DetailsModified : "")}{(hasChanges && hasAttachmentChanges ? " & " : "")}{(hasAttachmentChanges ? Common.AttachmentModified : "")}");
                    _notificationService.CreateNotification(ticket, 4, null, ticket.TicketAssignment?.AgentId);
                }
            }
            else
            {
                throw new TicketException(Errors.TicketDoesNotExist);
            }
        }

        /// <summary>
        /// Deletes a ticket asynchronously.
        /// Deletes all associated data (attachments, assignments, feedback, notifications, and comments).
        /// </summary>
        /// <param name="id">The ticket identifier.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="TicketException">Thrown when the ticket does not exist.</exception>
        public async Task DeleteAsync(string id)
        {
            var ticket = await _repository.FindByIdAsync(id);
            if (ticket != null)
            {
                await _repository.DeleteAsync(ticket);
            }
            else throw new TicketException(Errors.TicketDoesNotExist);
        }
        #endregion Ticket CRUD Operations

        #region Utility Methods
        /// <summary>
        /// Checks if a ticket is a duplicate asynchronously.
        /// </summary>
        /// <param name="ticket">The new ticket.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a boolean indicating whether the ticket is a duplicate.</returns>
        private async Task<bool> IsDuplicateTicketAsync(TicketViewModel ticket, string userId)
        {
            var duplicateTickets = await _repository.FindByUserIdAsync(userId);
            return duplicateTickets
                .Where(t => t.StatusTypeId != "S4")
                .Any(t => t.Subject.ToLower() == ticket.Subject.ToLower() && t.CategoryTypeId == ticket.CategoryTypeId);
        }

        /// <summary>
        /// Assigns properties to a new ticket.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
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
        /// Updates the ticket resolved date based on its status.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        private async Task UpdateTicketDate(Ticket ticket)
        {
            if (ticket.StatusTypeId != null && (ticket.StatusTypeId.Equals("S3") || ticket.StatusTypeId.Equals("S4")))
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
