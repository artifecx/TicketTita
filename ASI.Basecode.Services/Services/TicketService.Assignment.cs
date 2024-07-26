using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using static ASI.Basecode.Services.Exceptions.TicketExceptions;
using Microsoft.Extensions.Logging;
using ASI.Basecode.Resources.Messages;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Service class for handling operations related to ticket assignments.
    /// </summary>
    public partial class TicketService : ITicketService
    {
        /// <summary>
        /// Updates the assignment of a ticket asynchronously.
        /// </summary>
        /// <param name="model">The ticket view model.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the assignment type: "assign", "unassign", or "reassign".</returns>
        /// <exception cref="TicketException">Thrown when the assignment update is invalid.</exception>
        public async Task<string> UpdateAssignmentAsync(TicketViewModel model)
        {
            var currentUser = _httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var status = string.Empty;
            var ticketId = model.TicketId;
            var teamId = model.TeamId;
            var agentId = model.AgentId;
            var assignment = await _repository.FindAssignmentByTicketIdAsync(ticketId);
            const string noTeam = "no_team";
            const string noAgent = "no_agent";
            string activityLogDetail = string.Empty;

            if (assignment == null)
            {
                status = "assign";
                if (teamId == noTeam && agentId == noAgent)
                {
                    throw new TicketException(Errors.SelectAgentOrTeam, ticketId);
                }

                if (teamId == noTeam && agentId != noAgent)
                {
                    assignment = await CreateTicketAssignmentAsync(ticketId, agentId);
                    activityLogDetail = Common.TicketAssignedToAgent;
                }
                else if (teamId != noTeam && agentId == noAgent)
                {
                    assignment = await CreateTicketAssignmentAsync(ticketId, teamId: teamId);
                    activityLogDetail = Common.TicketAssignedToTeam;
                }
                else if (teamId != noTeam && agentId != noAgent)
                {
                    assignment = await CreateTicketAssignmentAsync(ticketId, agentId, teamId);
                    activityLogDetail = Common.TicketAssignedToTeamAndAgent;
                }
                else
                {
                    throw new TicketException(Errors.InvalidAssignmentUpdate, ticketId);
                }
                await _repository.AssignTicketAsync(assignment);
            }
            else
            {
                var assignmentTeamId = assignment.TeamId;
                var assignmentAgentId = assignment.AgentId;
                if (teamId == assignmentTeamId && agentId == assignmentAgentId)
                {
                    throw new TicketException(Errors.CannotReassignSameTeamAndAgent, ticketId);
                }
                if (teamId == assignmentTeamId && agentId == noAgent && assignmentAgentId == null)
                {
                    throw new TicketException(Errors.CannotAssignSameTeamWithoutAgent, ticketId);
                }

                if (teamId == noTeam && agentId == noAgent)
                {
                    status = "unassign";
                    await _repository.RemoveAssignmentAsync(assignment);
                    return status;
                }

                if (teamId == assignmentTeamId && agentId == noAgent)
                {
                    status = "unassign";
                    assignment.AgentId = null;
                    activityLogDetail = Common.AgentUnassignedFromTicket;
                }
                else if (teamId == noTeam && agentId != assignmentAgentId)
                {
                    status = "reassign";
                    assignment.TeamId = null;
                    assignment.AgentId = agentId;
                    activityLogDetail = Common.NewAgentAssignedToTicket;
                }
                else if (teamId == assignmentTeamId && agentId != assignmentAgentId)
                {
                    status = "reassign";
                    assignment.AgentId = agentId;
                    activityLogDetail = Common.NewAgentAssignedToTicket;
                }
                else if (teamId != assignmentTeamId && agentId == noAgent)
                {
                    status = "reassign";
                    assignment.TeamId = teamId;
                    assignment.AgentId = null;
                    activityLogDetail = Common.NewTeamAssignedToTicket;
                }
                else if (teamId != assignmentTeamId && agentId != assignmentAgentId)
                {
                    status = "reassign";
                    assignment.TeamId = teamId;
                    assignment.AgentId = agentId;
                    activityLogDetail = Common.NewTeamAndAgentAssignedToTicket;
                }
                else if (string.IsNullOrEmpty(assignmentTeamId) && !string.IsNullOrEmpty(teamId) && agentId == assignmentAgentId)
                {
                    status = "assign";
                    assignment.TeamId = teamId;
                    activityLogDetail = Common.NewTeamAssignedToTicket;
                }
                else
                {
                    throw new TicketException(Errors.InvalidAssignmentUpdate, ticketId);
                }
                assignment.AssignedDate = DateTime.Now;
                assignment.AssignedById = currentUser;
                await _repository.UpdateAssignmentAsync(assignment);
            }
            await CheckAndModifyStatusByAssignment(ticketId, status);
            var ticket = await _repository.FindByIdAsync(model.TicketId);
            await _activityLogService.LogActivityAsync(ticket, currentUser, Common.AssignmentUpdated, activityLogDetail);
            _notificationService.CreateNotification(ticket, 5, status == "reassign", ticket.TicketAssignment?.AgentId);
            return status;
        }

        /// <summary>
        /// Checks the ticket's new assignment status and modifies its status type accordingly.
        /// </summary>
        /// <param name="ticketId">The ticket identifier.</param>
        /// <param name="status">The status.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="TicketException">Thrown when the status is invalid.</exception>
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
                        throw new TicketException(string.Format(Errors.CannotAssignInClosedTickets, status, statusType), ticketId);
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
                        throw new TicketException(string.Format(Errors.CannotAssignInClosedTickets, status, statusType), ticketId);
                    if (statusType == "open")
                        break;
                    if (statusType == "in progress")
                    {
                        ticket.StatusTypeId = "S1";
                        await UpdateTrackingAsync(ticketT: ticket);
                    }
                    break;
                default:
                    throw new TicketException(string.Format(Errors.InvalidStatusValue, status), status);
            }
        }

        /// <summary>
        /// Creates a new ticket assignment asynchronously.
        /// </summary>
        /// <param name="ticketId">The ticket identifier.</param>
        /// <param name="agentId">The agent identifier.</param>
        /// <param name="teamId">The team identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the new ticket assignment.</returns>
        /// <exception cref="TicketException">Thrown when the current user is not found.</exception>
        private async Task<TicketAssignment> CreateTicketAssignmentAsync(string ticketId, string agentId = null, string teamId = null)
        {
            var currentUser = _httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUser == null)
                throw new TicketException(Errors.CurrentUserNotFound);

            return new TicketAssignment
            {
                AssignmentId = Guid.NewGuid().ToString(),
                AgentId = agentId,
                TeamId = teamId,
                TicketId = ticketId,
                AssignedDate = DateTime.Now,
                AssignedById = currentUser,
            };
        }
    }
}
