using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ASI.Basecode.Services.Exceptions.TicketExceptions;
using static ASI.Basecode.Services.Exceptions.TeamExceptions;
using System.Linq;
using System.Security.Claims;
using System.Data.Entity;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ASI.Basecode.Services.Services
{
    public partial class TicketService : ITicketService
    {
        /// <summary>
        /// Calls the repository to get all tickets.
        /// </summary>
        /// <returns>IEnumerable TicketViewModel</returns>
        public async Task<List<TicketViewModel>> GetAllAsync()
        {
            var tickets = await _repository.GetAllAsync();
            var ticketViewModels = _mapper.Map<List<TicketViewModel>>(tickets);

            return ticketViewModels;
        }

        /// <summary>
        /// Calls the repository to get a ticket by its identifier.
        /// </summary>
        /// <param name="id">The identifier</param>
        /// <returns>TicketViewModel</returns>
        public async Task<TicketViewModel> GetTicketByIdAsync(string id)
        {
            var ticket = await _repository.FindByIdAsync(id);
            var mappedTicket = _mapper.Map<TicketViewModel>(ticket);

            return mappedTicket;
        }

        /// <summary>
        /// Gets the ticket and filters it
        /// </summary>
        /// <param name="id">The identifier</param>
        /// <returns>TicketViewModel</returns>
        public async Task<TicketViewModel> GetFilteredTicketByIdAsync(string id)
        {
            var currentUserId = _httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = _httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.Role)?.Value;
            if (currentUserId == null || currentUserRole == null) return null;

            var ticket = await GetTicketByIdAsync(id);
            if (ticket == null) return null;
            if (ticket.StatusTypeId == "S4" && ticket.UserId != currentUserId) return null;
            
            if (currentUserRole.Contains("Support Agent"))
            {
                var agent = await _teamRepository.FindAgentByIdAsync(currentUserId);
                if (ticket.CategoryType.CategoryName != "Others" &&
                    ticket.TicketAssignment?.TeamId != agent.TeamMember?.TeamId &&
                    ticket.TicketAssignment?.AgentId != agent.UserId &&
                    ticket.CategoryTypeId != agent.TeamMember?.Team.SpecializationId)
                    return null;
            }

            var agents = await _teamRepository.GetAgentsAsync();
            var teams = await _teamRepository.GetAllStrippedAsync();
            var statusTypes = await GetStatusTypesAsync();
            var categoryTypes = await GetCategoryTypesAsync();
            var priorityTypes = await GetPriorityTypesAsync();

            ticket.StatusTypes = currentUserRole.Contains("Employee") ? statusTypes.Where(x => x.StatusName != "Resolved" && x.StatusName != "In Progress") :
                    statusTypes.Where(x => x.StatusName != "Closed" && !(ticket.Agent == null && x.StatusName == "Resolved"));
            ticket.PriorityTypes = priorityTypes;
            ticket.CategoryTypes = categoryTypes;
            ticket.Comments = ticket.Comments?.OrderByDescending(c => c.PostedDate);
            ticket.Teams = currentUserRole.Contains("Admin") ? teams : teams.Where(x => x.TeamMembers != null && x.TeamMembers.Any(x => x.UserId == currentUserId));
            ticket.Agents = agents;
            ticket.AgentsWithNoTeam = currentUserRole.Contains("Admin") ? agents.Where(x => x.TeamMember == null) : null;

            return ticket;
        }

        /// <summary>
        /// Calls GetAllAsync and filters the result based on sortBy, filterBy, and filterValue.
        /// </summary>
        /// <param name="sortBy">User defined sort order</param>
        /// <param name="filterBy">User defined filter category</param>
        /// <param name="filterValue">User defined filter value</param>
        /// <returns>IEnumerable TicketViewModel</returns>
        public async Task<PaginatedList<TicketViewModel>> GetFilteredAndSortedTicketsAsync(string sortBy, string filterBy, string filterValue, string search, int pageIndex, int pageSize)
        {
            var tickets = await GetAllAsync();
            var userRole = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userRole) && userRole.Contains("Employee"))
            {
                tickets = tickets.Where(x => x.UserId == userId).ToList();
            }
            else if (!string.IsNullOrEmpty(userRole) && userRole.Contains("Support Agent"))
            {
                var agent = await _teamRepository.FindAgentByIdAsync(userId);

                var agentTeamId = agent.TeamMember?.TeamId;
                var teamSpecializationId = agent.TeamMember?.Team.SpecializationId;

                var assignedToAgentTeam = new List<TicketViewModel>();
                var openTicketsForTeam = new List<TicketViewModel>();
                var assignedToAgentNoTeam = new List<TicketViewModel>();
                var openTicketsNoTeam = new List<TicketViewModel>();
                var openTicketsOthers = new List<TicketViewModel>();

                foreach (var ticket in tickets)
                {
                    /// Agent is assigned to a team
                    if(agentTeamId != null)
                    {
                        if (ticket.TicketAssignment?.TeamId == agentTeamId)
                        {
                            assignedToAgentTeam.Add(ticket);
                        }
                    }
                    /// Agent is not assigned to a team
                    else
                    {
                        if (ticket.TicketAssignment?.AgentId == userId && ticket.TicketAssignment?.TeamId == null)
                        {
                            assignedToAgentNoTeam.Add(ticket);
                        }
                    }

                    /// Ticket is open and not assigned to a team/agent
                    if (ticket.TicketAssignment == null && ticket.StatusType.StatusName != "Resolved" && ticket.StatusType.StatusName != "Closed")
                    {
                        /// Ticket falls under a team specialization
                        if (ticket.CategoryTypeId == teamSpecializationId)
                        {
                            openTicketsForTeam.Add(ticket);
                        }
                        /// Ticket falls under the "Others" category
                        else if (ticket.CategoryType.CategoryName == "Others")
                        {
                            openTicketsOthers.Add(ticket);
                        }
                        /// All open tickets not assigned to a team/agent
                        else
                        {
                            openTicketsNoTeam.Add(ticket);
                        }
                    }
                }

                /// Join relevant tickets based on agent's team membership
                if (agent.TeamMember != null)
                {
                    tickets = assignedToAgentTeam.Union(openTicketsForTeam).Union(openTicketsOthers).ToList();
                }
                else
                {
                    tickets = assignedToAgentNoTeam.Union(openTicketsNoTeam).Union(openTicketsOthers).ToList();
                }
            } 
            else if (!string.IsNullOrEmpty(userRole) && userRole.Contains("Admin"))
            {
                tickets = tickets.Where(x => x.StatusType.StatusName != "Closed").ToList();
            }

            if (!string.IsNullOrEmpty(filterBy) && !string.IsNullOrEmpty(filterValue))
            {
                tickets = filterBy.ToLower() switch
                {
                    "priority" => tickets.Where(t => t.PriorityType.PriorityName == filterValue).ToList(),
                    "status" => tickets.Where(t => t.StatusType.StatusName == filterValue).ToList(),
                    "category" => tickets.Where(t => t.CategoryType.CategoryName == filterValue).ToList(),
                    "user" => tickets.Where(t => t.User.Name == filterValue).ToList(),
                    _ => tickets
                };
            }

            if (!string.IsNullOrEmpty(search))
            {
                tickets = tickets.Where(t => t.Subject.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            tickets = sortBy switch
            {
                "id_desc" => tickets.OrderByDescending(t => t.TicketId).ToList(),
                "subject_desc" => tickets.OrderByDescending(t => t.Subject).ToList(),
                "subject" => tickets.OrderBy(t => t.Subject).ToList(),
                "status_desc" => tickets.OrderByDescending(t => t.StatusTypeId).ToList(),
                "status" => tickets.OrderBy(t => t.StatusTypeId).ToList(),
                "priority_desc" => tickets.OrderByDescending(t => t.PriorityTypeId).ToList(),
                "priority" => tickets.OrderBy(t => t.PriorityTypeId).ToList(),
                "category_desc" => tickets.OrderByDescending(t => t.CategoryType.CategoryName).ToList(),
                "category" => tickets.OrderBy(t => t.CategoryType.CategoryName).ToList(),
                "user_desc" => tickets.OrderByDescending(t => t.User.Name).ToList(),
                "user" => tickets.OrderBy(t => t.User.Name).ToList(),
                "created_desc" => tickets.OrderByDescending(t => t.CreatedDate).ToList(),
                "created" => tickets.OrderBy(t => t.CreatedDate).ToList(),
                "updated_desc" => tickets.OrderByDescending(t => t.UpdatedDate).ToList(),
                "updated" => tickets.OrderBy(t => t.UpdatedDate).ToList(),
                "resolved_desc" => tickets.OrderByDescending(t => t.ResolvedDate).ToList(),
                "resolved" => tickets.OrderBy(t => t.ResolvedDate).ToList(),
                _ => tickets.OrderBy(t => t.TicketId).ToList(),
            };

            var count = tickets.Count;
            var items = tickets.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            return new PaginatedList<TicketViewModel>(items, count, pageIndex, pageSize);
        }

        /// <summary>
        /// Calls the repository to get an attachment by ticket identifier.
        /// </summary>
        /// <param name="id">Ticket identifier</param>
        /// <returns>Attachment</returns>
        public async Task<Attachment> GetAttachmentByTicketIdAsync(string id)
            => await _repository.FindAttachmentByTicketIdAsync(id);

        /// <summary>
        /// Calls the repository to get a ticket assignment by ticket identifier.
        /// </summary>
        /// <param name="id">Ticket identifier</param>
        /// <returns>TicketAssignment</returns>
        public async Task<TicketAssignment> GetAssignmentByTicketIdAsync(string id)
            => await _repository.FindAssignmentByTicketIdAsync(id);

        /// <summary>
        /// Calls the repository to get a team by user identifier.
        /// </summary>
        /// <param name="id">User identifier</param>
        /// <returns>Team</returns>
        public async Task<Team> GetTeamByUserIdAsync(string id)
            => await _repository.FindTeamByUserIdAsync(id);

        /// <summary>
        /// Calls the repository to get all categories.
        /// </summary>
        /// <returns>IEnumerable CategoryType</returns>
        public async Task<IEnumerable<CategoryType>> GetCategoryTypesAsync()
            => await _repository.GetCategoryTypesAsync();

        /// <summary>
        /// Calls the repository to get all priority types.
        /// </summary>
        /// <returns>IEnumerable PriorityType</returns>
        public async Task<IEnumerable<PriorityType>> GetPriorityTypesAsync()
            => await _repository.GetPriorityTypesAsync();

        /// <summary>
        /// Calls the repository to get all status types.
        /// </summary>
        /// <returns>IEnumerable StatusType</returns>
        public async Task<IEnumerable<StatusType>> GetStatusTypesAsync()
            => await _repository.GetStatusTypesAsync();

        /// <summary>
        /// Calls the repository to get all users with role "Support Aagent".
        /// </summary>
        /// <returns>IEnumerable User</returns>
        public async Task<IEnumerable<User>> GetSupportAgentsAsync()
            => await _repository.GetSupportAgentsAsync();

        /// <summary>
        /// Gets the unresolved tickets older than.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <returns></returns>
        public IEnumerable<TicketViewModel> GetUnresolvedTicketsOlderThan(TimeSpan timeSpan)
        {
            var cutoffTime = DateTime.Now.Subtract(timeSpan);

            var unresolvedTickets = _repository.GetAllAsync().Result
                .Where(t => (t.ResolvedDate == null) && t.CreatedDate <= cutoffTime && (t.User.UserId != null))
                .ToList();
            if (unresolvedTickets.Count == 0)
            {
                Console.WriteLine("============================ No unresolved tickets older than ============================ " + timeSpan);
            }
            return unresolvedTickets.Select(ticket => _mapper.Map<TicketViewModel>(ticket));
        }

        /// <summary>
        /// Calls the repository to get all users with tickets.
        /// </summary>
        /// <returns>IEnumerable string</returns>
        public async Task<IEnumerable<string>> GetUserIdsWithTicketsAsync()
            => await _repository.GetUserIdsWithTicketsAsync();

        /// <summary>
        /// Calls the repository to get all users.
        /// </summary>
        /// <returns>IEnumerable user</returns>
        public async Task<IEnumerable<User>> UserGetAllAsync()
            => await _repository.UserGetAllAsync();
    }
}
