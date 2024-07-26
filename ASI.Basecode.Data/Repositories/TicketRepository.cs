using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    /// <summary>
    /// Repository class for handling operations related to the Ticket entity.
    /// </summary>
    public class TicketRepository : BaseRepository, ITicketRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TicketRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public TicketRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        #region Ticket Service Methods
        /// <summary>
        /// Retrieves tickets with necessary related data.
        /// Includes:
        /// CategoryType, PriorityType, StatusType, User, Feedback, Attachments, ActivityLogs->User, Comments->User, Comments->Parent, Comments->InverseParent, TicketAssignment->Agent, TicketAssignment->Team.
        /// </summary>
        /// <returns>An <see cref="IQueryable{T}"/> of <see cref="Ticket"/> including the specified related data.</returns>
        private IQueryable<Ticket> GetTicketsWithIncludes()
        {
            return this.GetDbSet<Ticket>()
                    .Where(t => !t.IsDeleted)
                    .Include(t => t.CategoryType)
                    .Include(t => t.PriorityType)
                    .Include(t => t.StatusType)
                    .Include(t => t.User)
                    .Include(t => t.Feedback)
                    .Include(t => t.Attachments)
                    .Include(t => t.ActivityLogs)
                        .ThenInclude(t => t.User)
                    .Include(t => t.Comments)
                        .ThenInclude(cu => cu.User)
                    .Include(t => t.Comments)
                        .ThenInclude(cp => cp.Parent)
                    .Include(t => t.Comments)
                        .ThenInclude(c => c.InverseParent)
                    .Include(t => t.TicketAssignment)
                        .ThenInclude(ta => ta.Agent)
                    .Include(t => t.TicketAssignment)
                        .ThenInclude(ta => ta.Team);
        }

        /// <summary>
        /// Retrieves tickets with limited related data.
        /// Includes:
        /// CategoryType, PriorityType, StatusType, User, Feedback, ActivityLogs->User, TicketAssignment->Agent, TicketAssignment->Team.
        /// </summary>
        /// <returns>An <see cref="IQueryable{T}"/> of <see cref="Ticket"/> including the specified related data.</returns>
        private IQueryable<Ticket> GetTicketsWithLimitedIncludes()
        {
            return this.GetDbSet<Ticket>()
                    .Where(t => !t.IsDeleted)
                    .Include(t => t.CategoryType)
                    .Include(t => t.PriorityType)
                    .Include(t => t.StatusType)
                    .Include(t => t.User)
                    .Include(t => t.Feedback)
                    .Include(t => t.ActivityLogs)
                        .ThenInclude(t => t.User)
                    .Include(t => t.TicketAssignment)
                        .ThenInclude(ta => ta.Agent)
                    .Include(t => t.TicketAssignment)
                        .ThenInclude(ta => ta.Team);
        }

        /// <summary>
        /// Retrieves all tickets asynchronously with limited related data.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a list of tickets.</returns>
        public async Task<List<Ticket>> GetAllAsync() =>
            await GetTicketsWithLimitedIncludes().AsNoTracking().ToListAsync();

        /// <summary>
        /// Retrieves all tickets including deleted tickets asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a list of tickets, including deleted ones.</returns>
        public async Task<List<Ticket>> GetAllAndDeletedTicketsAsync()
        {
            return await this.GetDbSet<Ticket>()
                    .AsNoTracking()
                    .Include(t => t.Feedback)
                    .Include(t => t.TicketAssignment)
                        .ThenInclude(ta => ta.Agent)
                    .Include(t => t.TicketAssignment)
                        .ThenInclude(ta => ta.Team)
                        .ToListAsync();
        }

        /// <summary>
        /// Counts all tickets including deleted tickets asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the count of tickets, including deleted ones.</returns>
        public async Task<int> CountAllAndDeletedTicketsAsync() =>
            await this.GetDbSet<Ticket>().AsNoTracking().CountAsync();

        /// <summary>
        /// Adds a new ticket asynchronously.
        /// </summary>
        /// <param name="ticket">The ticket to add.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddAsync(Ticket ticket)
        {
            await this.GetDbSet<Ticket>().AddAsync(ticket);
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing ticket asynchronously.
        /// </summary>
        /// <param name="ticket">The ticket to update.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateAsync(Ticket ticket)
        {
            this.GetDbSet<Ticket>().Update(ticket);
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Soft deletes a ticket asynchronously.
        /// </summary>
        /// <param name="ticket">The ticket to delete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteAsync(Ticket ticket)
        {
            ticket.IsDeleted = true;
            this.GetDbSet<Ticket>().Update(ticket);
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Hard deletes a ticket asynchronously.
        /// </summary>
        /// <param name="ticket">The ticket to delete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteHardAsync(Ticket ticket)
        {
            ticket.CategoryTypeId = null;
            ticket.PriorityTypeId = null;
            ticket.StatusTypeId = null;
            ticket.UserId = null;

            this.GetDbSet<Ticket>().Remove(ticket);
            await UnitOfWork.SaveChangesAsync();
        }
        #endregion Ticket Service Methods

        #region Attachment Service Methods
        /// <summary>
        /// Adds an attachment to a ticket asynchronously.
        /// </summary>
        /// <param name="attachment">The attachment to add.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddAttachmentAsync(Attachment attachment)
        {
            await this.GetDbSet<Attachment>().AddAsync(attachment);
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Removes an attachment from a ticket asynchronously.
        /// </summary>
        /// <param name="attachment">The attachment to remove.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RemoveAttachmentAsync(Attachment attachment)
        {
            this.GetDbSet<Attachment>().Remove(attachment);
            await UnitOfWork.SaveChangesAsync();
        }
        #endregion Attachment Service Methods

        #region Ticket Assignment Service Methods  
        /// <summary>
        /// Adds a ticket assignment asynchronously.
        /// </summary>
        /// <param name="assignment">The assignment to add.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AssignTicketAsync(TicketAssignment assignment)
        {
            await this.GetDbSet<TicketAssignment>().AddAsync(assignment);
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing ticket assignment asynchronously.
        /// </summary>
        /// <param name="assignment">The assignment to update.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateAssignmentAsync(TicketAssignment assignment)
        {
            this.GetDbSet<TicketAssignment>().Update(assignment);
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Removes a ticket assignment asynchronously.
        /// </summary>
        /// <param name="assignment">The assignment to remove.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task RemoveAssignmentAsync(TicketAssignment assignment)
        {
            this.GetDbSet<TicketAssignment>().Remove(assignment);
            await UnitOfWork.SaveChangesAsync();
        }
        #endregion Ticket Assignment Service Methods

        #region Comment Service Methods        
        /// <summary>
        /// Adds a comment to a ticket asynchronously.
        /// </summary>
        /// <param name="comment">The comment to add.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddCommentAsync(Comment comment)
        {
            this.GetDbSet<Comment>().Add(comment);
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an existing comment asynchronously.
        /// </summary>
        /// <param name="comment">The comment to update.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateCommentAsync(Comment comment)
        {
            this.GetDbSet<Comment>().Update(comment);
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a comment by identifier asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the comment to delete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteCommentAsync(string id)
        {
            var comment = await FindCommentByIdAsync(id);
            await DeleteCommentAndChildrenRecursive(comment);
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Recursively deletes a comment and its child comments.
        /// </summary>
        /// <param name="comment">The comment to delete.</param>
        private async Task DeleteCommentAndChildrenRecursive(Comment comment)
        {
            await Context.Entry(comment).Collection(c => c.InverseParent).LoadAsync();

            foreach (var childComment in comment.InverseParent.ToList())
            {
                await DeleteCommentAndChildrenRecursive(childComment);
                this.GetDbSet<Comment>().Remove(childComment);
            }

            this.GetDbSet<Comment>().Remove(comment);
        }

        /// <summary>
        /// Retrieves comments with necessary related data.
        /// Includes: User, Parent, InverseParent.
        /// </summary>
        /// <returns>An <see cref="IQueryable{T}"/> of <see cref="Comment"/> including the specified related data.</returns>
        public IQueryable<Comment> GetCommentsWithIncludesAsync()
        {
            return this.GetDbSet<Comment>()
                                .Include(cu => cu.User)
                                .Include(cp => cp.Parent)
                                .Include(c => c.InverseParent);
        }

        /// <summary>
        /// Finds a comment by identifier asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the comment to find.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the comment with the specified identifier.</returns>
        public async Task<Comment> FindCommentByIdAsync(string id) =>
            await GetCommentsWithIncludesAsync().FirstOrDefaultAsync(c => c.CommentId == id);
        #endregion Comment Service Methods

        #region Find Methods
        /// <summary>
        /// Finds a ticket by identifier asynchronously.
        /// </summary>
        /// <param name="id">The ticket identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the ticket with the specified identifier.</returns>
        public async Task<Ticket> FindByIdAsync(string id)
            => await GetTicketsWithIncludes().FirstOrDefaultAsync(t => t.TicketId == id);

        /// <summary>
        /// Finds tickets by user identifier asynchronously.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a list of tickets associated with the specified user identifier.</returns>
        public async Task<IEnumerable<Ticket>> FindByUserIdAsync(string id) =>
            await GetTicketsWithIncludes().Where(t => t.UserId == id).ToListAsync();

        /// <summary>
        /// Finds an attachment by ticket identifier asynchronously.
        /// </summary>
        /// <param name="id">The ticket identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the attachment associated with the specified ticket identifier.</returns>
        public async Task<Attachment> FindAttachmentByTicketIdAsync(string id)
            => await this.GetDbSet<Attachment>().FirstOrDefaultAsync(x => x.TicketId == id);

        /// <summary>
        /// Finds a ticket assignment by ticket identifier asynchronously.
        /// </summary>
        /// <param name="id">The ticket identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the ticket assignment associated with the specified ticket identifier.</returns>
        public async Task<TicketAssignment> FindAssignmentByTicketIdAsync(string id)
            => await this.GetDbSet<TicketAssignment>().FirstOrDefaultAsync(x => x.TicketId == id);

        /// <summary>
        /// Finds a team by a member's user identifier asynchronously.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the team associated with the specified member's user identifier.</returns>
        public async Task<Team> FindTeamByUserIdAsync(string id)
        {
            var memberOf = await this.GetDbSet<TeamMember>().FirstOrDefaultAsync(x => x.UserId == id);
            return await this.GetDbSet<Team>().FirstOrDefaultAsync(x => x.TeamId == memberOf.TeamId);
        }

        /// <summary>
        /// Finds a priority type by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The priority type identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the priority type with the specified identifier.</returns>
        public async Task<PriorityType> FindPriorityByIdAsync(string id)
            => await this.GetDbSet<PriorityType>().FirstOrDefaultAsync(x => x.PriorityTypeId == id);

        /// <summary>
        /// Finds a status type by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The status type identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the status type with the specified identifier.</returns>
        public async Task<StatusType> FindStatusByIdAsync(string id)
            => await this.GetDbSet<StatusType>().FirstOrDefaultAsync(x => x.StatusTypeId == id);

        /// <summary>
        /// Finds a user by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the user with the specified identifier.</returns>
        public async Task<User> UserFindByIdAsync(string id)
            => await this.GetDbSet<User>().FirstOrDefaultAsync(x => x.UserId == id);

        /// <summary>
        /// Finds a feedback by ticket identifier asynchronously.
        /// </summary>
        /// <param name="id">The ticket identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the feedback associated with the specified ticket identifier.</returns>
        public async Task<Feedback> FeedbackFindByTicketIdAsync(string id)
            => await this.GetDbSet<Feedback>().FirstOrDefaultAsync(x => x.TicketId == id);

        /// <summary>
        /// Finds an admin by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The admin identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the admin with the specified identifier.</returns>
        public async Task<Admin> AdminFindByIdAsync(string id)
            => await this.GetDbSet<Admin>().FirstOrDefaultAsync(x => x.AdminId == id);
        #endregion Find Methods

        #region Get Methods
        /// <summary>
        /// Retrieves all category types asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains an <see cref="IQueryable{T}"/> of <see cref="CategoryType"/>.</returns>
        public async Task<IQueryable<CategoryType>> GetCategoryTypesAsync()
            => await Task.FromResult(this.GetDbSet<CategoryType>());

        /// <summary>
        /// Retrieves all priority types asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains an <see cref="IQueryable{T}"/> of <see cref="PriorityType"/>.</returns>
        public async Task<IQueryable<PriorityType>> GetPriorityTypesAsync()
            => await Task.FromResult(this.GetDbSet<PriorityType>());

        /// <summary>
        /// Retrieves all status types asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains an <see cref="IQueryable{T}"/> of <see cref="StatusType"/>.</returns>
        public async Task<IQueryable<StatusType>> GetStatusTypesAsync()
            => await Task.FromResult(this.GetDbSet<StatusType>());

        /// <summary>
        /// Retrieves all support agents asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains an <see cref="IQueryable{T}"/> of <see cref="User"/> with the role of Support Agent.</returns>
        public async Task<IQueryable<User>> GetSupportAgentsAsync()
            => await Task.FromResult(this.GetDbSet<User>().Where(x => x.RoleId == "Support Agent"));

        /// <summary>
        /// Retrieves all users asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains an <see cref="IQueryable{T}"/> of <see cref="User"/> including their tickets.</returns>
        public async Task<IQueryable<User>> UserGetAllAsync()
            => await Task.FromResult(this.GetDbSet<User>().Include(x => x.Tickets));

        /// <summary>
        /// Retrieves all user identifiers who have submitted tickets asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains an <see cref="IQueryable{T}"/> of <see cref="string"/> representing the user identifiers.</returns>
        public async Task<IQueryable<string>> GetUserIdsWithTicketsAsync()
            => await Task.FromResult(this.GetDbSet<Ticket>().Select(x => x.UserId).Distinct());
        #endregion Get Methods
    }
}
