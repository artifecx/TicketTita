using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;
using static ASI.Basecode.Services.Exceptions.TicketExceptions;

namespace ASI.Basecode.Services.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IActivityLogRepository _repository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="mapper">The mapper.</param>
        public ActivityLogService(IActivityLogRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        /// <summary>
        /// Logs the activity asynchronous.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="activityType">Type of the activity.</param>
        /// <param name="details">The details.</param>
        public async Task LogActivityAsync(Ticket ticket, string userId, string activityType, string details)
        {
            var activityLog = new ActivityLog
            {
                ActivityId = Guid.NewGuid().ToString(),
                TicketId = ticket.TicketId,
                UserId = userId,
                ActivityType = activityType,
                ActivityDate = DateTime.Now,
                Details = details,
            };

            // Add the log entry to the ticket's activity logs
            ticket.ActivityLogs.Add(activityLog);

            await _repository.AddActivityLogAsync(activityLog);
        }

        /// <summary>
        /// Retrieves all activity logs associated with a specific ticket.
        /// </summary>
        /// <param name="ticketId">The identifier of the ticket</param>
        /// <returns>A list of activity logs for the specified ticket</returns>
        public async Task<IEnumerable<ActivityLog>> GetActivityLogsByTicketIdAsync(string ticketId)
        {
            if (string.IsNullOrEmpty(ticketId))
            {
                throw new ArgumentException("Ticket ID cannot be null or empty.", nameof(ticketId));
            }

            // Fetch the activity logs from the repository
            var activityLogs = await _repository.GetActivityLogsByTicketIdAsync(ticketId);

            if (activityLogs == null)
            {
                throw new TicketException("No activity logs found for the specified ticket.");
            }

            return activityLogs;
        }
    }
}
