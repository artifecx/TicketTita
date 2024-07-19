using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ASI.Basecode.Services.Exceptions.TicketExceptions;
using static ASI.Basecode.Services.Exceptions.TeamExceptions;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace ASI.Basecode.Services.Services
{
    public partial class TicketService : ITicketService
    {
        /// <summary>
        /// Updates the assignment.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>string assignment type: "assign", "unassign", "reassign"</returns>
        /// new assignment
        /// case 0: no team, no agent - exception
        /// case 1: no team, new agent
        /// case 2: new team, no agent
        /// case 3: new team, new agent
        /// ------------------------------
        /// existing assignment
        /// case 0: same team, same agent - exception
        /// case 1: no team, no agent
        /// case 2: same team, no agent
        /// case 3: no team, different agent
        /// case 4: same team, different agent
        /// case 5: different team, no agent
        /// case 6: different team, different agent
        /// case 7: new team from no team, same agent -> no team agent with ticket was assigned to a team
        public async Task<string> UpdateAssignmentAsync(TicketViewModel model)
        {
            var currentUser = _httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var status = string.Empty;
            var ticketId = model.TicketId;
            var teamId = model.TeamId;
            var agentId = model.AgentId;
            var assignment = await _repository.FindAssignmentByTicketIdAsync(ticketId);
            string noTeam = "no_team";
            string noAgent = "no_agent";

            if (assignment == null)
            {
                status = "assign";
                if (teamId == noTeam && agentId == noAgent)
                {
                    throw new TicketException("Please select either an agent or a team to continue.", ticketId);
                }

                if (teamId == noTeam && agentId != noAgent)
                {
                    assignment = await CreateTicketAssignmentAsync(ticketId, agentId);
                }
                else if (teamId != noTeam && agentId == noAgent)
                {
                    assignment = await CreateTicketAssignmentAsync(ticketId, teamId: teamId);
                }
                else if (teamId != noTeam && agentId != noAgent)
                {
                    assignment = await CreateTicketAssignmentAsync(ticketId, agentId, teamId);
                }
                else
                {
                    throw new TicketException("Invalid assignment update. Please try again.", ticketId);
                }
                await _repository.AssignTicketAsync(assignment);
            }
            else
            {
                var assignmentTeamId = assignment.TeamId;
                var assignmentAgentId = assignment.AgentId;
                if (teamId == assignmentTeamId && agentId == assignmentAgentId)
                {
                    throw new TicketException("Cannot reassign to the same team and agent.", ticketId);
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
                }
                else if (teamId == noTeam && agentId != assignmentAgentId)
                {
                    status = "reassign";
                    assignment.TeamId = null;
                    assignment.AgentId = agentId;
                }
                else if (teamId == assignmentTeamId && agentId != assignmentAgentId)
                {
                    status = "reassign";
                    assignment.AgentId = agentId;
                }
                else if (teamId != assignmentTeamId && agentId == noAgent)
                {
                    status = "reassign";
                    assignment.TeamId = teamId;
                    assignment.AgentId = null;
                }
                else if (teamId != assignmentTeamId && agentId != assignmentAgentId)
                {
                    status = "reassign";
                    assignment.TeamId = teamId;
                    assignment.AgentId = agentId;
                }
                else if(string.IsNullOrEmpty(assignmentTeamId) && 
                    !string.IsNullOrEmpty(teamId) && agentId == assignmentAgentId)
                {
                    status = "assign";
                    assignment.TeamId = teamId;
                }
                else
                {
                    throw new TicketException("Invalid assignment update. Please try again.", ticketId);
                }
                assignment.AssignedDate = DateTime.Now;
                assignment.AssignedById = currentUser;
                await _repository.UpdateAssignmentAsync(assignment);
            }
            await CheckAndModifyStatusByAssignment(ticketId, status);
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
        private async Task<TicketAssignment> CreateTicketAssignmentAsync(string ticketId, string agentId = null, string teamId = null)
        {
            var currentUser = _httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUser == null)
                throw new TicketException("Current user not found, unable to proceed.");

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
