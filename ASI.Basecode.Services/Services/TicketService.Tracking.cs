using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using static ASI.Basecode.Services.Exceptions.TicketExceptions;
using ASI.Basecode.Resources.Messages;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Service class for handling operations related to tickets.
    /// </summary>
    public partial class TicketService : ITicketService
    {
        /// <summary>
        /// Updates the tracking information of a ticket, including its status and/or priority.
        /// </summary>
        /// <param name="ticketV">The ticket view model.</param>
        /// <param name="ticketT">The ticket entity.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="TicketException">Thrown when the ticket is not found, no changes are made, or invalid updates are attempted on closed tickets.</exception>
        public async Task UpdateTrackingAsync(TicketViewModel ticketV = null, Ticket ticketT = null)
        {
            var ticket = ticketV ?? _mapper.Map<TicketViewModel>(ticketT);
            var existingTicket = await _repository.FindByIdAsync(ticket.TicketId);
            if (existingTicket == null)
                throw new TicketException(Errors.TicketNotFound);

            var currentStatus = existingTicket.StatusType.StatusName.ToLower();
            var closedStatusList = new List<string> { "resolved", "closed" };

            bool statusChanged = existingTicket.StatusTypeId != ticket.StatusTypeId;
            bool priorityChanged = existingTicket.PriorityTypeId != ticket.PriorityTypeId && ticket.PriorityTypeId != null;

            if (!statusChanged && !priorityChanged && ticketT == null)
                throw new TicketException(Errors.TicketNoChanges, ticket.TicketId);

            if ((statusChanged && priorityChanged) && currentStatus == "closed" && ticketT == null)
                throw new TicketException(Errors.TicketClosedUpdate, ticket.TicketId);

            if (statusChanged)
            {
                if (currentStatus == "closed")
                    throw new TicketException(Errors.TicketClosedStatusUpdate, ticket.TicketId);

                existingTicket.StatusTypeId = string.IsNullOrEmpty(ticket.StatusTypeId) ?
                    existingTicket.StatusTypeId : ticket.StatusTypeId;
            }

            if (priorityChanged)
            {
                if (closedStatusList.Contains(currentStatus))
                    throw new TicketException(Errors.TicketClosedPriorityUpdate, ticket.TicketId);

                existingTicket.PriorityTypeId = string.IsNullOrEmpty(ticket.PriorityTypeId) ?
                    existingTicket.PriorityTypeId : ticket.PriorityTypeId;
            }

            existingTicket.UpdatedDate = DateTime.Now;
            await UpdateTicketDate(existingTicket);
            await _repository.UpdateAsync(existingTicket);

            if (statusChanged || priorityChanged)
            {
                await _activityLogService.LogActivityAsync(existingTicket, _httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier).Value, Common.TicketUpdate,
                    $"{(statusChanged ? "Status" : "")}{(statusChanged && priorityChanged ? " & " : "")}{(priorityChanged ? "Priority" : "")} modified");
                _notificationService.CreateNotification(existingTicket, statusChanged ? 3 : 2, null, existingTicket.TicketAssignment?.AgentId);
                _notificationService.CreateNotification(existingTicket, priorityChanged && !statusChanged ? 2 : 3, null, existingTicket.TicketAssignment?.TeamId);

                if (existingTicket.StatusTypeId == "S3" && existingTicket.TicketAssignment?.AgentId != null)
                    await _performanceReportService.GenerateAgentPerformanceReportAsync(existingTicket.TicketAssignment.AgentId);
            }
        }
    }
}
