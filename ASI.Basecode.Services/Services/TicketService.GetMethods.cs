using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ASI.Basecode.Resources.Messages;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Service class for handling operations related to tickets.
    /// </summary>
    public partial class TicketService : ITicketService
    {
        /// <summary>
        /// Retrieves all tickets asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a list of ticket view models.</returns>
        public async Task<List<TicketViewModel>> GetAllAsync()
        {
            var tickets = await _repository.GetAllAsync();
            var ticketViewModels = _mapper.Map<List<TicketViewModel>>(tickets);
            return ticketViewModels;
        }

        /// <summary>
        /// Retrieves a ticket by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The ticket identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the ticket view model.</returns>
        public async Task<TicketViewModel> GetTicketByIdAsync(string id)
        {
            var ticket = await _repository.FindByIdAsync(id);
            var mappedTicket = _mapper.Map<TicketViewModel>(ticket);
            return mappedTicket;
        }

        /// <summary>
        /// Retrieves and filters a ticket by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The ticket identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the filtered ticket view model.</returns>
        public async Task<TicketViewModel> GetFilteredTicketByIdAsync(string id)
        {
            var currentUserId = _httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = _httpContextAccessor.HttpContext.User?.FindFirst(ClaimTypes.Role)?.Value;
            if (currentUserId == null || currentUserRole == null) return null;

            var ticket = await GetTicketByIdAsync(id);
            if (ticket == null) return null;

            var agents = await _teamRepository.GetAgentsAsync();
            var teams = await _teamRepository.GetAllStrippedAsync();
            var statusTypes = await GetStatusTypesAsync();
            var categoryTypes = await GetCategoryTypesAsync();
            var priorityTypes = await GetPriorityTypesAsync();

            ticket.StatusTypes = !currentUserRole.Contains("Employee") ? statusTypes.Where(x => x.StatusName != "Closed") : statusTypes.Where(x => x.StatusName != "Closed" && x.StatusName != "In Progress");
            ticket.PriorityTypes = priorityTypes;
            ticket.CategoryTypes = categoryTypes;
            ticket.Comments = ticket.Comments?.OrderByDescending(c => c.PostedDate);
            ticket.Teams = currentUserRole.Contains("Admin") ? teams : teams.Where(x => x.TeamMembers != null && x.TeamMembers.Any(x => x.UserId == currentUserId));
            ticket.Agents = agents;
            ticket.AgentsWithNoTeam = !currentUserRole.Contains("Employee") ? agents.Where(x => x.TeamMember == null) : null;

            return ticket;
        }

        /// <summary>
        /// Retrieves filtered and sorted tickets asynchronously.
        /// </summary>
        /// <param name="showOption">User defined show option.</param>
        /// <param name="sortBy">User defined sort order.</param>
        /// <param name="selectedFilters">User defined selected filters.</param>
        /// <param name="search">Search string.</param>
        /// <param name="clearFilters">Indicates whether to clear filters.</param>
        /// <param name="pageIndex">Page index.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a paginated list of ticket view models.</returns>
        public async Task<PaginatedList<TicketViewModel>> GetFilteredAndSortedTicketsAsync(string showOption, string sortBy, List<string> selectedFilters, string search, bool clearFilters, int pageIndex, int pageSize)
        {
            var tickets = await GetAllAsync();
            var userRole = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            var userId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userPreferences = _userPreferencesRepository.GetUserPreferences(userId);
            userPreferences.TryGetValue(UserPreferences.DefaultShowOption, out var defaultShowOption);
            userPreferences.TryGetValue(UserPreferences.DefaultSortBy, out var defaultSortBy);
            userPreferences.TryGetValue(UserPreferences.DefaultStatusFilter, out var defaultStatusFilter);
            userPreferences.TryGetValue(UserPreferences.DefaultPriorityFilter, out var defaultPriorityFilter);
            userPreferences.TryGetValue(UserPreferences.DefaultCategoryFilter, out var defaultCategoryFilter);

            if (!string.IsNullOrEmpty(userRole) && userRole.Contains(UserRoles.Employee))
            {
                tickets = tickets.Where(x => x.UserId == userId).ToList();
            }
            else if (!string.IsNullOrEmpty(userRole) && userRole.Contains(UserRoles.SupportAgent))
            {
                var agent = await _teamRepository.FindAgentByIdAsync(userId);
                showOption = string.IsNullOrEmpty(showOption) ? defaultShowOption : showOption;

                if (!clearFilters)
                {
                    tickets = showOption?.ToLower() switch
                    {
                        "assigned_me" => tickets.Where(t => t.TicketAssignment?.AgentId == userId).ToList(),
                        "assigned_team" => tickets.Where(t => agent.TeamMember != null && t.TicketAssignment?.TeamId == agent.TeamMember?.TeamId).ToList(),
                        "assigned_none" => tickets.Where(t => t.TicketAssignment == null).ToList(),
                        _ => tickets
                    };
                }
            }

            if (!string.IsNullOrEmpty(search))
            {
                tickets = tickets.Where(t => t.Subject.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!clearFilters)
            {
                sortBy = string.IsNullOrEmpty(sortBy) ? defaultSortBy : sortBy;

                if (defaultStatusFilter != null && (selectedFilters.Count == 0 || !selectedFilters.Exists(f => f != null && f.StartsWith("status:"))))
                {
                    selectedFilters.Add("status:" + defaultStatusFilter);
                }
                if (defaultPriorityFilter != null && (selectedFilters.Count == 0 || !selectedFilters.Exists(f => f != null && f.StartsWith("priority:"))))
                {
                    selectedFilters.Add("priority:" + defaultPriorityFilter);
                }
                if (defaultCategoryFilter != null && (selectedFilters.Count == 0 || !selectedFilters.Exists(f => f != null && f.StartsWith("category:"))))
                {
                    selectedFilters.Add("category:" + defaultCategoryFilter);
                }

                if (selectedFilters.Any())
                {
                    foreach (var filter in selectedFilters)
                    {
                        if (filter == null) continue;
                        var filterParts = filter.Split(":");
                        var filterBy = filterParts[0];
                        var filterValue = filterParts[1];

                        if (filterValue == "all") continue;
                        tickets = filterBy switch
                        {
                            "status" => tickets.Where(x => x.StatusTypeId == filterValue).ToList(),
                            "priority" => tickets.Where(x => x.PriorityTypeId == filterValue).ToList(),
                            "category" => tickets.Where(x => x.CategoryTypeId == filterValue).ToList(),
                            "employee" => tickets.Where(x => x.UserId == filterValue).ToList(),
                            "agent" => tickets.Where(x => x.TicketAssignment?.AgentId == filterValue).ToList(),
                            "team" => tickets.Where(x => x.TicketAssignment?.TeamId == filterValue).ToList(),
                            _ => tickets
                        };
                    }
                }
            }

            tickets = sortBy?.ToLower() switch
            {
                "ticket_desc" => tickets.OrderByDescending(t => t.TicketId).ToList(),
                "ticket_asc" => tickets.OrderBy(t => t.TicketId).ToList(),
                "subject_desc" => tickets.OrderByDescending(t => t.Subject).ToList(),
                "subject_asc" => tickets.OrderBy(t => t.Subject).ToList(),
                "created_desc" => tickets.OrderByDescending(t => t.CreatedDate).ToList(),
                "created_asc" => tickets.OrderBy(t => t.CreatedDate).ToList(),
                "updated_desc" => tickets.OrderByDescending(t => t.UpdatedDate).ToList(),
                _ => tickets.OrderByDescending(t => t.UpdatedDate).ToList(),
            };

            var count = tickets.Count;
            var items = tickets.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            return new PaginatedList<TicketViewModel>(items, count, pageIndex, pageSize);
        }

        /// <summary>
        /// Retrieves an attachment by ticket identifier asynchronously.
        /// </summary>
        /// <param name="id">The ticket identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the attachment.</returns>
        public async Task<Attachment> GetAttachmentByTicketIdAsync(string id)
            => await _repository.FindAttachmentByTicketIdAsync(id);

        /// <summary>
        /// Retrieves a ticket assignment by ticket identifier asynchronously.
        /// </summary>
        /// <param name="id">The ticket identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the ticket assignment.</returns>
        public async Task<TicketAssignment> GetAssignmentByTicketIdAsync(string id)
            => await _repository.FindAssignmentByTicketIdAsync(id);

        /// <summary>
        /// Retrieves a team by user identifier asynchronously.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the team.</returns>
        public async Task<Team> GetTeamByUserIdAsync(string id)
            => await _repository.FindTeamByUserIdAsync(id);

        /// <summary>
        /// Retrieves all category types asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains an enumerable of category types.</returns>
        public async Task<IEnumerable<CategoryType>> GetCategoryTypesAsync()
            => await _repository.GetCategoryTypesAsync();

        /// <summary>
        /// Retrieves all priority types asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains an enumerable of priority types.</returns>
        public async Task<IEnumerable<PriorityType>> GetPriorityTypesAsync()
            => await _repository.GetPriorityTypesAsync();

        /// <summary>
        /// Retrieves all status types asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains an enumerable of status types.</returns>
        public async Task<IEnumerable<StatusType>> GetStatusTypesAsync()
            => await _repository.GetStatusTypesAsync();

        /// <summary>
        /// Retrieves all support agents asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains an enumerable of users with the role of Support Agent.</returns>
        public async Task<IEnumerable<User>> GetSupportAgentsAsync()
            => await _repository.GetSupportAgentsAsync();

        /// <summary>
        /// Retrieves unresolved tickets older than the specified time span.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <returns>An enumerable of ticket view models.</returns>
        public IEnumerable<TicketViewModel> GetUnresolvedTicketsOlderThan(TimeSpan timeSpan)
        {
            var cutoffTime = DateTime.Now.Subtract(timeSpan);

            var unresolvedTickets = _repository.GetAllAsync().Result
                .Where(t => (t.ResolvedDate == null) && t.CreatedDate <= cutoffTime && (t.User.UserId != null))
                .ToList();
            if (unresolvedTickets.Count == 0)
            {
                Console.WriteLine(string.Format(Errors.NoUnresolvedTickets, timeSpan));
            }
            return unresolvedTickets.Select(ticket => _mapper.Map<TicketViewModel>(ticket));
        }

        /// <summary>
        /// Retrieves all user identifiers with tickets asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains an enumerable of user identifiers.</returns>
        public async Task<IEnumerable<string>> GetUserIdsWithTicketsAsync()
            => await _repository.GetUserIdsWithTicketsAsync();

        /// <summary>
        /// Retrieves all users asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains an enumerable of users.</returns>
        public async Task<IEnumerable<User>> UserGetAllAsync()
            => await _repository.UserGetAllAsync();
    }
}
