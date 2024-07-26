using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    /// <summary>
    /// Repository class for handling operations related to the Team entity.
    /// </summary>
    public class TeamRepository : BaseRepository, ITeamRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public TeamRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        /// <summary>
        /// Retrieves teams with necessary related data.
        /// Includes: 
        /// TeamMembers->User, 
        /// TeamMembers->User->PerformanceReport, 
        /// TeamMembers->User->TicketAssignmentAgents->Ticket->Feedback, 
        /// TicketAssignments->Agent, 
        /// TicketAssignments->Ticket->Feedback, 
        /// Specialization
        /// </summary>
        /// <returns>An <see cref="IQueryable{T}"/> of <see cref="Team"/> including the specified related data.</returns>
        private IQueryable<Team> GetTeamsWithIncludes()
        {
            return this.GetDbSet<Team>()
                        .Where(t => !t.IsDeleted)
                        .Include(t => t.TeamMembers)
                            .ThenInclude(u => u.User)
                        .Include(t => t.TeamMembers)
                            .ThenInclude(tm => tm.User)
                            .ThenInclude(tm => tm.PerformanceReport)
                        .Include(t => t.TeamMembers)
                            .ThenInclude(tm => tm.User)
                            .ThenInclude(tm => tm.TicketAssignmentAgents)
                            .ThenInclude(ta => ta.Ticket)
                            .ThenInclude(ta => ta.Feedback)
                        .Include(t => t.TicketAssignments)
                            .ThenInclude(a => a.Agent)
                        .Include(t => t.TicketAssignments)
                            .ThenInclude(t => t.Ticket)
                            .ThenInclude(t => t.Feedback)
                        .Include(t => t.Specialization);
        }

        /// <summary>
        /// Retrieves all teams asynchronously with necessary related data.
        /// Includes: 
        /// TeamMembers->User, 
        /// TeamMembers->User->PerformanceReport, 
        /// TeamMembers->User->TicketAssignmentAgents->Ticket->Feedback, 
        /// TicketAssignments->Agent, 
        /// TicketAssignments->Ticket->Feedback, 
        /// Specialization
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a list of teams.</returns>
        public async Task<List<Team>> GetAllAsync() =>
            await GetTeamsWithIncludes().AsNoTracking().ToListAsync();

        /// <summary>
        /// Adds a new team asynchronously.
        /// </summary>
        /// <param name="team">The team to add.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddAsync(Team team)
        {
            await this.GetDbSet<Team>().AddAsync(team);
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the team asynchronously.
        /// </summary>
        /// <param name="team">The team to update.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateAsync(Team team)
        {
            this.GetDbSet<Team>().Update(team);
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Soft deletes the team asynchronously.
        /// </summary>
        /// <param name="team">The team to delete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteAsync(Team team)
        {
            team.IsDeleted = true;
            this.GetDbSet<Team>().Update(team);
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Adds a team member asynchronously.
        /// </summary>
        /// <param name="teamMember">The team member to add.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddTeamMemberAsync(TeamMember teamMember)
        {
            await this.GetDbSet<TeamMember>().AddAsync(teamMember);
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Removes a team member asynchronously.
        /// </summary>
        /// <param name="teamMember">The team member to remove.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RemoveTeamMemberAsync(TeamMember teamMember)
        {
            this.GetDbSet<TeamMember>().Remove(teamMember);
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves all teams with lightweight includes asynchronously.
        /// Includes: 
        /// TeamId, Name, 
        /// TeamMembers->UserId, 
        /// TeamMembers->User->Name, 
        /// Specialization->CategoryTypeId, 
        /// Specialization->CategoryName
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a list of lightweight teams.</returns>
        public async Task<IEnumerable<Team>> GetAllStrippedAsync()
        {
            return this.GetDbSet<Team>()
                        .Where(team => !team.IsDeleted)
                        .Select(team => new
                        {
                            team.TeamId,
                            team.Name,
                            TeamSpecialization = new CategoryType
                            {
                                CategoryTypeId = team.Specialization.CategoryTypeId,
                                CategoryName = team.Specialization.CategoryName
                            },
                            TeamMembers = team.TeamMembers
                                .Select(member => new
                                {
                                    member.UserId,
                                    member.User.Name
                                })
                        })
                        .AsEnumerable()
                        .Select(t => new Team
                        {
                            TeamId = t.TeamId,
                            Name = t.Name,
                            TeamMembers = t.TeamMembers.Select(m => new TeamMember
                            {
                                UserId = m.UserId,
                                User = new User
                                {
                                    Name = m.Name
                                }
                            })
                            .ToList(),
                            Specialization = t.TeamSpecialization
                        });
        }

        /// <summary>
        /// Retrieves the lightweight version of users with the role of Support Agent asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a list of users with the role of Support Agent.</returns>
        public async Task<IEnumerable<User>> GetAgentsAsync()
        {
            return await this.GetDbSet<User>()
                        .Include(a => a.TeamMember)
                        .Where(a => a.RoleId.Equals("Support Agent"))
                        .Select(agent => new User
                        {
                            UserId = agent.UserId,
                            Name = agent.Name,
                            Email = agent.Email,
                            TeamMember = agent.TeamMember
                        }).ToListAsync();
        }

        /// <summary>
        /// Finds a team by identifier asynchronously.
        /// Includes: 
        /// TeamMembers->User, 
        /// TeamMembers->User->PerformanceReport, 
        /// TeamMembers->User->TicketAssignmentAgents->Ticket->Feedback, 
        /// TicketAssignments->Agent, 
        /// TicketAssignments->Ticket->Feedback, 
        /// Specialization
        /// </summary>
        /// <param name="id">The team identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the team with the specified identifier, including related data.</returns>
        public async Task<Team> FindByIdAsync(string id) =>
            await GetTeamsWithIncludes().FirstOrDefaultAsync(t => t.TeamId == id);

        /// <summary>
        /// Finds a support agent by identifier asynchronously.
        /// Includes: 
        /// CreatedByNavigation, 
        /// Role, 
        /// UpdatedByNavigation, 
        /// TeamMember, 
        /// PerformanceReport, 
        /// ActivityLogs, 
        /// KnowledgeBaseArticles, 
        /// TicketAssignmentAgents->Ticket
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the user with the specified identifier, including related data.</returns>
        public async Task<User> FindAgentByIdAsync(string id)
        {
            return await this.GetDbSet<User>()
                        .Where(u => u.RoleId.Equals("Support Agent"))
                        .Include(u => u.CreatedByNavigation)
                        .Include(u => u.Role)
                        .Include(u => u.UpdatedByNavigation)
                        .Include(u => u.TeamMember)
                            .ThenInclude(tm => tm.Team)
                        .Include(u => u.PerformanceReport)
                        .Include(u => u.ActivityLogs)
                        .Include(u => u.KnowledgeBaseArticles)
                        .Include(u => u.TicketAssignmentAgents)
                            .ThenInclude(ta => ta.Ticket)
                        .FirstOrDefaultAsync(u => u.UserId == id);
        }

        /// <summary>
        /// Finds a team member by identifier asynchronously.
        /// </summary>
        /// <param name="id">The team member identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the team member with the specified identifier, including related data.</returns>
        public async Task<TeamMember> FindTeamMemberByIdAsync(string id) =>
            await this.GetDbSet<TeamMember>().Include(u => u.Team).FirstOrDefaultAsync(tm => tm.UserId == id);

        /// <summary>
        /// Determines whether the specified user is an existing team member in the specified team.
        /// </summary>
        /// <param name="teamId">The team identifier.</param>
        /// <param name="agentId">The agent identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a boolean value indicating whether the user is an existing team member in the team.</returns>
        public async Task<bool> IsExistingTeamMember(string teamId, string agentId) =>
            await GetTeamsWithIncludes()
                .AnyAsync(t => t.TeamMembers.Any(tm => tm.UserId == agentId && tm.TeamId == teamId));

        /// <summary>
        /// Retrieves completed tickets assigned to the specified agent asynchronously.
        /// </summary>
        /// <param name="agentId">The agent identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a list of completed tickets assigned to the agent.</returns>
        public async Task<List<Ticket>> GetCompletedTicketsAssignedToAgentAsync(string agentId)
        {
            return await this.GetDbSet<Ticket>()
                .Include(t => t.TicketAssignment)
                .Include(t => t.Feedback)
                    .ThenInclude(f => f.User)
                    .ThenInclude(f => f.PerformanceReport)
                .Where(t => (t.StatusTypeId == "S3" || t.StatusTypeId == "S4") && (t.TicketAssignment != null && t.TicketAssignment.AgentId == agentId))
                .ToListAsync();
        }
    }
}
