using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static ASI.Basecode.Services.Exceptions.TeamExceptions;
using static ASI.Basecode.Services.Exceptions.TicketExceptions;
using ASI.Basecode.Resources.Messages;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Service class for handling operations related to teams.
    /// </summary>
    public class TeamService : ITeamService
    {
        private readonly ITeamRepository _repository;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly ITicketService _ticketService;
        private readonly IPerformanceReportRepository _performanceReportRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamService"/> class.
        /// </summary>
        /// <param name="repository">The team repository.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="notificationService">The notification service.</param>
        /// <param name="ticketService">The ticket service.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="performanceReportRepository">The performance report repository.</param>
        public TeamService(
            ITeamRepository repository,
            IMapper mapper,
            INotificationService notificationService,
            ITicketService ticketService,
            ILogger<TeamService> logger,
            IHttpContextAccessor httpContextAccessor,
            IPerformanceReportRepository performanceReportRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _notificationService = notificationService;
            _ticketService = ticketService;
            _performanceReportRepository = performanceReportRepository;
        }

        /// <summary>
        /// Adds a new team asynchronously.
        /// </summary>
        /// <param name="team">The team view model.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="TeamException">Thrown when the specialization is invalid or the team name already exists.</exception>
        public async Task AddAsync(TeamViewModel team)
        {
            string[] validSpecializationIds = { "C1", "C2", "C3", "C4" };
            if (!validSpecializationIds.Contains(team.SpecializationId))
                throw new TeamException(Errors.InvalidSpecialization, team.TeamId);

            var teams = await _repository.GetAllAsync();
            if (teams.Any(t => t.Name.ToLower() == team.Name.ToLower()))
                throw new TeamException(Errors.TeamNameExists);

            if (team != null)
            {
                var newTeam = new Team
                {
                    TeamId = Guid.NewGuid().ToString(),
                    Name = team.Name,
                    Description = team.Description,
                    SpecializationId = team.SpecializationId
                };

                await _repository.AddAsync(newTeam);
            }
        }

        /// <summary>
        /// Updates an existing team asynchronously.
        /// </summary>
        /// <param name="team">The team view model.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="TeamException">Thrown when the specialization is invalid or no changes were made to the team.</exception>
        public async Task UpdateAsync(TeamViewModel team)
        {
            string[] validSpecializationIds = { "C1", "C2", "C3", "C4" };
            if (!validSpecializationIds.Contains(team.SpecializationId))
                throw new TeamException(Errors.InvalidSpecialization, team.TeamId);

            var existingTeam = await _repository.FindByIdAsync(team.TeamId);
            if (existingTeam != null)
            {
                bool hasChanges = existingTeam.Name != team.Name ||
                                  existingTeam.Description != team.Description ||
                                  existingTeam.SpecializationId != team.SpecializationId;

                if (!hasChanges)
                    throw new TeamException(Errors.NoChangesToTeam, team.TeamId);

                var teamMembers = existingTeam.TeamMembers;
                var ticketAssignments = existingTeam.TicketAssignments;
                var specialization = existingTeam.Specialization;

                _mapper.Map(team, existingTeam);

                existingTeam.TeamMembers = teamMembers;
                existingTeam.TicketAssignments = ticketAssignments;
                existingTeam.Specialization = specialization;

                await _repository.UpdateAsync(existingTeam);
            }
        }

        /// <summary>
        /// Deletes a team asynchronously.
        /// </summary>
        /// <param name="id">The team identifier.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="TeamException">Thrown when the team has unresolved tickets or members.</exception>
        public async Task DeleteAsync(string id)
        {
            var team = await _repository.FindByIdAsync(id);
            var teamMembers = team.TeamMembers.ToList();
            var ticketAssignments = team.TicketAssignments.ToList();

            if (ticketAssignments.Any(t => t.Ticket.ResolvedDate == null))
            {
                throw new TeamException(Errors.UnresolvedTicketsMessage);
            }
            if (teamMembers.Any())
            {
                throw new TeamException(Errors.MembersExistMessage);
            }

            await _repository.DeleteAsync(team);
        }

        /// <summary>
        /// Adds a team member to a team asynchronously.
        /// </summary>
        /// <param name="teamId">The team identifier.</param>
        /// <param name="agentId">The agent identifier.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="TeamException">Thrown when no agent is selected.</exception>
        public async Task AddTeamMemberAsync(string teamId, string agentId)
        {
            if (string.IsNullOrEmpty(agentId))
                throw new TeamException(Errors.SelectAgentToAdd, teamId);

            bool existingTeamMember = await _repository.IsExistingTeamMember(teamId, agentId);
            if (!existingTeamMember)
            {
                var team = await _repository.FindByIdAsync(teamId);
                var agent = await _repository.FindAgentByIdAsync(agentId);

                foreach (var ticketAssignment in agent.TicketAssignmentAgents)
                {
                    if (ticketAssignment.Ticket.StatusTypeId == "S3" || ticketAssignment.Ticket.StatusTypeId == "S4") continue;
                    var model = new TicketViewModel
                    {
                        AgentId = agentId,
                        TicketId = ticketAssignment.TicketId,
                        TeamId = teamId
                    };
                    await _ticketService.UpdateAssignmentAsync(model);
                }

                var teamMember = new TeamMember
                {
                    TeamId = team.TeamId,
                    UserId = agentId,
                    Team = team,
                    User = agent
                };

                team.TeamMembers.Add(teamMember);
                await _repository.AddTeamMemberAsync(teamMember);
                _notificationService.AddNotification(null, string.Format(Errors.AddedToTeamNotification, team.Name, team.Specialization?.CategoryName), "9", agentId, "Added to Team");
            }
        }

        /// <summary>
        /// Removes a team member from a team asynchronously.
        /// </summary>
        /// <param name="teamId">The team identifier.</param>
        /// <param name="agentId">The agent identifier.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RemoveTeamMemberAsync(string teamId, string agentId)
        {
            bool existingTeamMember = await _repository.IsExistingTeamMember(teamId, agentId);
            if (existingTeamMember)
            {
                var team = await _repository.FindByIdAsync(teamId);
                var agent = await _repository.FindAgentByIdAsync(agentId);
                var teamMember = await _repository.FindTeamMemberByIdAsync(agentId);

                foreach (var ticketAssignment in agent.TicketAssignmentAgents)
                {
                    if (ticketAssignment.Ticket.StatusTypeId == "S3" || ticketAssignment.Ticket.StatusTypeId == "S4") continue;
                    var model = new TicketViewModel
                    {
                        AgentId = "no_agent",
                        TicketId = ticketAssignment.TicketId,
                        TeamId = ticketAssignment.TeamId
                    };
                    await _ticketService.UpdateAssignmentAsync(model);
                }

                team.TeamMembers.Remove(teamMember);
                await _repository.RemoveTeamMemberAsync(teamMember);
                _notificationService.AddNotification(null, string.Format(Errors.RemovedFromTeamNotification, team.Name), "9", agentId, "Removed from Team");
            }
        }

        #region Get Methods        
        /// <summary>
        /// Retrieves all teams asynchronously with pagination and sorting options.
        /// </summary>
        /// <param name="sortBy">The sorting criteria.</param>
        /// <param name="filterBy">The filtering criteria.</param>
        /// <param name="specialization">The specialization filter.</param>
        /// <param name="pageIndex">The current page index.</param>
        /// <param name="pageSize">The size of each page.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a paginated list of team view models.</returns>
        public async Task<PaginatedList<TeamViewModel>> GetAllAsync(string sortBy, string filterBy, string specialization, int pageIndex, int pageSize)
        {
            var teams = _mapper.Map<List<TeamViewModel>>(await _repository.GetAllAsync());

            if (!string.IsNullOrEmpty(filterBy))
            {
                teams = teams.Where(team => team.Name.Contains(filterBy, StringComparison.OrdinalIgnoreCase) ||
                                   (team.Specialization.CategoryName.Contains(filterBy, StringComparison.OrdinalIgnoreCase)))
                             .ToList();
            }

            if (!string.IsNullOrEmpty(specialization))
            {
                teams = teams.Where(team => team.SpecializationId.Contains(specialization, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            foreach (var team in teams)
            {
                var averageResolutionTime = CalculateAverageAsync(team.TeamMembers, a => a.User?.PerformanceReport?.AverageResolutionTime);
                var averageFeedbackRating = CalculateAverageAsync(team.TeamMembers.SelectMany(t => t.User?.TicketAssignmentAgents), t => t.Ticket?.Feedback?.FeedbackRating);
                await Task.WhenAll(averageFeedbackRating, averageResolutionTime);
                team.AverageResolutionTime = (await averageResolutionTime).ToString();
                team.AverageFeedbackRating = (await averageFeedbackRating).ToString();
            }

            teams = sortBy switch
            {
                "name_desc" => teams.OrderByDescending(t => t.Name).ToList(),
                "agents_desc" => teams.OrderByDescending(t => t.TeamMembers?.Count() ?? 0).ToList(),
                "agents" => teams.OrderBy(t => t.TeamMembers?.Count() ?? 0).ToList(),
                "active_desc" => teams.OrderByDescending(t => t.TicketAssignments?.Count(ta => ta.Ticket?.ResolvedDate == null) ?? 0).ToList(),
                "active" => teams.OrderBy(t => t.TicketAssignments?.Count(ta => ta.Ticket?.ResolvedDate == null) ?? 0).ToList(),
                "inactive_desc" => teams.OrderByDescending(t => t.TicketAssignments?.Count(ta => ta.Ticket?.ResolvedDate != null) ?? 0).ToList(),
                "inactive" => teams.OrderBy(t => t.TicketAssignments?.Count(ta => ta.Ticket?.ResolvedDate != null) ?? 0).ToList(),
                "completion_desc" => teams.OrderByDescending(t => t.AverageResolutionTime).ToList(),
                "completion" => teams.OrderBy(t => t.AverageResolutionTime).ToList(),
                "rating_desc" => teams.OrderByDescending(t => t.AverageFeedbackRating).ToList(),
                "rating" => teams.OrderBy(t => t.AverageFeedbackRating).ToList(),
                _ => teams.OrderBy(t => t.Name).ToList(),
            };

            var count = teams.Count;
            var items = teams.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            return new PaginatedList<TeamViewModel>(items, count, pageIndex, pageSize);
        }

        /// <summary>
        /// Calculates the average of a collection asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the collection.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="valueSelector">The value selector.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the average value.</returns>
        private static Task<double> CalculateAverageAsync<T>(IEnumerable<T> collection, Func<T, double?> valueSelector)
        {
            return Task.Run(() =>
            {
                double total = 0.0;
                int validCount = 0;

                foreach (var item in collection)
                {
                    double? value = valueSelector(item);
                    if (value > 0.0)
                    {
                        total += value.Value;
                        validCount++;
                    }
                }

                return validCount > 0 ? total / validCount : 0.0;
            });
        }

        /// <summary>
        /// Retrieves all teams with only the necessary attributes asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains an enumerable of teams.</returns>
        public async Task<IEnumerable<Team>> GetAllStrippedAsync() =>
            await _repository.GetAllStrippedAsync();

        /// <summary>
        /// Retrieves a team by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The team identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the team view model.</returns>
        public async Task<TeamViewModel> GetTeamByIdAsync(string id)
        {
            var team = await _repository.FindByIdAsync(id);
            var teamViewModel = _mapper.Map<TeamViewModel>(team);

            return teamViewModel;
        }

        /// <summary>
        /// Retrieves all agents asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains an enumerable of users.</returns>
        public async Task<IEnumerable<User>> GetAgentsAsync() =>
            await _repository.GetAgentsAsync();
        #endregion Get Methods
    }
}
