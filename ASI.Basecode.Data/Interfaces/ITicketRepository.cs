using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface ITicketRepository
    {
        IQueryable<Ticket> GetAll();
        IQueryable<Ticket> GetTickets(string type, List<string> assignedTicketIds);
        string Add(Ticket ticket);
        string Update(Ticket ticket);
        void Delete(Ticket ticket);
        void AddAttachment(Attachment attachment);
        void RemoveAttachment(Attachment attachment);
        void AssignTicket(TicketAssignment assignment);
        void RemoveAssignment(TicketAssignment assignment);
        Ticket FindById(string id);
        Attachment FindAttachmentById(string id);
        Attachment FindAttachmentByTicketId(string id);
        TicketAssignment FindAssignmentByTicketId(string id);
        Team FindTeamByUserId(string id);
        User FindAgentByUserId(string id);
        CategoryType FindCategoryById(string id);
        PriorityType FindPriorityById(string id);
        StatusType FindStatusById(string id);
        IQueryable<CategoryType> GetCategoryTypes();
        IQueryable<PriorityType> GetPriorityTypes();
        IQueryable<StatusType> GetStatusTypes();
        IQueryable<User> GetSupportAgents();
        IQueryable<TicketAssignment> GetTicketAssignments();
    }
}
