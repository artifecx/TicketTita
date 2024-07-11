using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class TicketRepository : BaseRepository, ITicketRepository
    {
        public TicketRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        #region Ticket Service Methods
        private IQueryable<Ticket> GetTicketsWithIncludes()
        {
            return this.GetDbSet<Ticket>().Include(t => t.CategoryType)
                                          .Include(t => t.PriorityType)
                                          .Include(t => t.StatusType)
                                          .Include(t => t.User)
                                          .Include(t => t.Feedback)
                                          .Include(t => t.Attachments)
                                          .Include(t => t.TicketAssignment)
                                            .ThenInclude(ta => ta.Team)
                                            .ThenInclude(team => team.TeamMembers)
                                            .ThenInclude(tm => tm.User);
        }

        public async Task<List<Ticket>> GetAllAsync()
        {
            var tickets = await GetTicketsWithIncludes().ToListAsync();
            return tickets;
        }

        public async Task AddAsync(Ticket ticket)
        {
            await this.GetDbSet<Ticket>().AddAsync(ticket);
            await UnitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(Ticket ticket)
        {
            this.GetDbSet<Ticket>().Update(ticket);
            await UnitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Ticket ticket)
        {
            this.GetDbSet<Ticket>().Remove(ticket);
            await UnitOfWork.SaveChangesAsync();
        }
        #endregion Ticket Service Methods

        #region Attachment Service Methods
        public async Task AddAttachmentAsync(Attachment attachment)
        {
            await this.GetDbSet<Attachment>().AddAsync(attachment);
            await UnitOfWork.SaveChangesAsync();
        }

        public async Task RemoveAttachmentAsync(Attachment attachment)
        {
            this.GetDbSet<Attachment>().Remove(attachment);
            await UnitOfWork.SaveChangesAsync();
        }
        #endregion Attachment Service Methods

        #region Ticket Assignment Service Methods        
        public async Task AssignTicketAsync(TicketAssignment assignment)
        {
            await this.GetDbSet<TicketAssignment>().AddAsync(assignment);
            await UnitOfWork.SaveChangesAsync();
        }

        public async Task RemoveAssignmentAsync(TicketAssignment assignment)
        {
            this.GetDbSet<TicketAssignment>().Remove(assignment);
            await UnitOfWork.SaveChangesAsync();
        }
        #endregion Ticket Assignment Service Methods

        #region Find Methods
        public async Task<Ticket> FindByIdAsync(string id)
        {
            var ticket = await GetTicketsWithIncludes()
                .Include(t => t.Attachments)
                .FirstOrDefaultAsync(t => t.TicketId == id);

            return ticket;
        }

        public async Task<Attachment> FindAttachmentByIdAsync(string id) => await this.GetDbSet<Attachment>().FirstOrDefaultAsync(x => x.AttachmentId == id);

        public async Task<Attachment> FindAttachmentByTicketIdAsync(string id) => await this.GetDbSet<Attachment>().FirstOrDefaultAsync(x => x.TicketId == id);

        public async Task<TicketAssignment> FindAssignmentByTicketIdAsync(string id) => await this.GetDbSet<TicketAssignment>().FirstOrDefaultAsync(x => x.TicketId == id);

        public async Task<Team> FindTeamByUserIdAsync(string id)
        {
            var memberOf = await this.GetDbSet<TeamMember>().FirstOrDefaultAsync(x => x.UserId == id);
            return await this.GetDbSet<Team>().FirstOrDefaultAsync(x => x.TeamId == memberOf.TeamId);
        }

        public async Task<User> FindAgentByUserIdAsync(string id) => await this.GetDbSet<User>().FirstOrDefaultAsync(x => x.UserId == id);

        public async Task<CategoryType> FindCategoryByIdAsync(string id) => await this.GetDbSet<CategoryType>().FirstOrDefaultAsync(x => x.CategoryTypeId == id);

        public async Task<PriorityType> FindPriorityByIdAsync(string id) => await this.GetDbSet<PriorityType>().FirstOrDefaultAsync(x => x.PriorityTypeId == id);

        public async Task<StatusType> FindStatusByIdAsync(string id) => await this.GetDbSet<StatusType>().FirstOrDefaultAsync(x => x.StatusTypeId == id);

        public async Task<IQueryable<CategoryType>> GetCategoryTypesAsync() => await Task.FromResult(this.GetDbSet<CategoryType>());

        public async Task<IQueryable<PriorityType>> GetPriorityTypesAsync() => await Task.FromResult(this.GetDbSet<PriorityType>());

        public async Task<IQueryable<StatusType>> GetStatusTypesAsync() => await Task.FromResult(this.GetDbSet<StatusType>());

        public async Task<IQueryable<User>> GetSupportAgentsAsync() => await Task.FromResult(this.GetDbSet<User>().Where(x => x.RoleId == "Support Agent"));

        public async Task<IQueryable<TicketAssignment>> GetTicketAssignmentsAsync() => await Task.FromResult(this.GetDbSet<TicketAssignment>());

        public async Task<IQueryable<string>> GetUserIdsWithTicketsAsync() => await Task.FromResult(this.GetDbSet<Ticket>().Select(x => x.UserId).Distinct());
        #endregion

        public async Task<IQueryable<User>> UserGetAllAsync() => await Task.FromResult(this.GetDbSet<User>());
        public async Task<User> UserFindByIdAsync (string id) => await this.GetDbSet<User>().FirstOrDefaultAsync(x => x.UserId == id);

        public async Task FeedbackDeleteAsync(Feedback feedback)
        {
            this.GetDbSet<Feedback>().Remove(feedback);
            await UnitOfWork.SaveChangesAsync();
        }

        public async Task<Feedback> FeedbackFindByTicketIdAsync(string id) => await this.GetDbSet<Feedback>().FirstOrDefaultAsync(x => x.TicketId == id);
        public async Task<Admin> AdminFindByIdAsync(string id) => await this.GetDbSet<Admin>().FirstOrDefaultAsync(x => x.AdminId == id);
    }
}
