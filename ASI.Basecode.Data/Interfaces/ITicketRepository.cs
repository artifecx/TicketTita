using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface ITicketRepository
    {
        Task<List<Ticket>> GetAllAsync();
        Task<List<Ticket>> GetAllAndDeletedAsync();
        Task AddAsync(Ticket ticket);
        Task UpdateAsync(Ticket ticket);
        Task DeleteAsync(Ticket ticket);
        Task AddAttachmentAsync(Attachment attachment);
        Task RemoveAttachmentAsync(Attachment attachment);
        Task AssignTicketAsync(TicketAssignment assignment);
        Task UpdateAssignmentAsync(TicketAssignment assignment);
        Task RemoveAssignmentAsync(TicketAssignment assignment);
        Task AddCommentAsync(Comment comment);
        Task UpdateCommentAsync(Comment comment);
        Task DeleteCommentAsync(string id);
        Task<Comment> FindCommentByIdAsync(string id);
        Task<Ticket> FindByIdAsync(string id);
        Task<IEnumerable<Ticket>> FindByUserIdAsync(string id);
        Task<Attachment> FindAttachmentByTicketIdAsync(string id);
        Task<TicketAssignment> FindAssignmentByTicketIdAsync(string id);
        Task<Team> FindTeamByUserIdAsync(string id);
        Task<PriorityType> FindPriorityByIdAsync(string id);
        Task<StatusType> FindStatusByIdAsync(string id);
        Task<IQueryable<CategoryType>> GetCategoryTypesAsync();
        Task<IQueryable<PriorityType>> GetPriorityTypesAsync();
        Task<IQueryable<StatusType>> GetStatusTypesAsync();
        Task<IQueryable<User>> GetSupportAgentsAsync();
        Task<IQueryable<string>> GetUserIdsWithTicketsAsync();
        Task<IQueryable<User>> UserGetAllAsync();
        Task<User> UserFindByIdAsync(string id);
        Task<Feedback> FeedbackFindByTicketIdAsync(string id);
        Task<Admin> AdminFindByIdAsync(string id);
    }
}
