using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Services.Interfaces
{
    public interface ITicketService
    {
        void Add(TicketViewModel ticket, string userId);
        void Update(TicketViewModel ticket);
        void Delete(string id);
        void AddAttachment(Attachment attachment);
        void RemoveAttachment(string attachmentId);
        void AddTicketAssignment(TicketViewModel model);
        void RemoveAssignment(string id);
        IQueryable<TicketViewModel> GetAll();
        TicketViewModel GetTicketById(string id);
        IQueryable<TicketViewModel> GetUnresolvedTickets();
        IQueryable<TicketViewModel> GetTicketsByAssignmentStatus(string status);
        Object GetTicketDetails(string id);
        Attachment GetAttachmentByTicketId(string id);
        TicketAssignment GetAssignmentByTicketId(string id);
        Team GetTeamByUserId(string id);
        User GetAgentById(string id);
        IQueryable<CategoryType> GetCategoryTypes();
        IQueryable<StatusType> GetStatusTypes();
        IQueryable<PriorityType> GetPriorityTypes();
        IQueryable<User> GetSupportAgents();
        IQueryable<TicketAssignment> GetTicketAssignments();
        TicketViewModel InitializeModel(string type);
        string ExtractAgentId(string id);
    }
}
