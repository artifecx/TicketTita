using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ASI.Basecode.Services.Exceptions.TicketExceptions;
using static ASI.Basecode.Services.Exceptions.TeamExceptions;
using System.Security.Claims;

namespace ASI.Basecode.Services.Services
{
    public partial class TicketService : ITicketService
    {
        /// <summary>
        /// Updates the assignment.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>string assignment type: "assign", "unassign", "reassign"</returns>
        public async Task<string> UpdateAssignmentAsync(TicketViewModel model)
        {
            var status = string.Empty;
            var assignment = await _repository.FindAssignmentByTicketIdAsync(model.TicketId);
            if (assignment != null)
            {
                if (model.AgentId == "remove")
                {
                    status = "unassign";
                    await CheckAndModifyStatusByAssignment(model.TicketId, status);
                    await _repository.RemoveAssignmentAsync(assignment);
                }
                else
                {
                    if (model.AgentId == ExtractAgentId(assignment.AssignmentId))
                        throw new TicketException("Cannot reassign to the same agent.", model.TicketId);

                    status = "reassign";
                    await CheckAndModifyStatusByAssignment(model.TicketId, status);

                    var ticket = await _repository.FindByIdAsync(model.TicketId);
                    await _repository.RemoveAssignmentAsync(assignment);

                    assignment = await CreateTicketAssignmentAsync(model.TicketId, model.AgentId);
                    ticket.TicketAssignment = assignment;
                    ticket.UpdatedDate = DateTime.Now;
                    assignment.Ticket = ticket;
                    await _repository.AssignTicketAsync(assignment);

                    CreateNotification(ticket, null, true, model.AgentId);
                    var team = _teamRepository.FindTeamMemberByIdAsync(model.AgentId).Result.Team;
                    await LogActivityAsync(ticket, _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value, "Update Assignment", $"Changed Assignment to: {team.Name}");
                }
            }
            else
            {
                if (model.AgentId == "remove")
                    throw new TicketException("Please select an agent to continue.", model.TicketId);

                status = "assign";
                await CheckAndModifyStatusByAssignment(model.TicketId, status);

                var ticket = await _repository.FindByIdAsync(model.TicketId);
                assignment = await CreateTicketAssignmentAsync(model.TicketId, model.AgentId);
                ticket.TicketAssignment = assignment;
                ticket.UpdatedDate = DateTime.Now;
                assignment.Ticket = ticket;
                await _repository.AssignTicketAsync(assignment);

                CreateNotification(ticket, null, false, model.AgentId);
            }
            return status;
        }

        /// <summary>
        /// Checks the ticket's new assignment status and modify its status type accordingly.
        /// </summary>
        /// <param name="ticketId">The ticket identifier.</param>
        /// <param name="status">The status.</param>
        private async Task CheckAndModifyStatusByAssignment(string ticketId, string status)
        {
            var ticket = await _repository.FindByIdAsync(ticketId);
            var statusType = ticket.StatusType.StatusName.ToLower();
            var closedStatuses = new List<string> { "resolved", "closed" };

            switch (status)
            {
                case "assign":
                case "reassign":
                    if (closedStatuses.Contains(statusType))
                        throw new TicketException($"Cannot {status} assignees in {statusType} tickets.", ticketId);
                    if (statusType == "in progress")
                        break;
                    if (statusType == "open")
                    {
                        ticket.StatusTypeId = "S2";
                        await UpdateTrackingAsync(ticketT: ticket);
                    }
                    break;
                case "unassign":
                    if (closedStatuses.Contains(statusType))
                        throw new TicketException($"Cannot unassign assignees in {statusType} tickets.", ticketId);
                    if (statusType == "open")
                        break;
                    if (statusType == "in progress")
                    {
                        ticket.StatusTypeId = "S1";
                        await UpdateTrackingAsync(ticketT: ticket);
                    }
                    break;
                default:
                    throw new TicketException(status, $"Invalid status value: {status}");
            }
        }

        /// <summary>
        /// Helper method to create a new ticket assignment.
        /// </summary>
        /// <param name="model">The model</param>
        /// <returns>TicketAssignment</returns>
        /// AssignmentId format: {AgentId}-{Timestamp}-{RandomNumber}
        private async Task<TicketAssignment> CreateTicketAssignmentAsync(string ticketId, string agentId)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var randomNumber = new Random().Next(1000, 9999);
            var assignmentId = $"{agentId}-{timestamp}-{randomNumber}";
            var currentAdmin = await GetCurrentAdminAsync();
            var team = await GetTeamByUserIdAsync(agentId);

            return new TicketAssignment
            {
                AssignmentId = assignmentId,
                TeamId = team.TeamId,
                TicketId = ticketId,
                AssignedDate = DateTime.Now,
                AdminId = currentAdmin.AdminId, //TODO: remove, agent can now reassign
                Team = team,
                Admin = currentAdmin
            };
        }
    }
}
