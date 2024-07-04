using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

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
        IEnumerable<TicketViewModel> GetAll();
        TicketViewModel GetTicketById(string id);
        IEnumerable<TicketViewModel> GetUnresolvedTickets();
        IEnumerable<TicketViewModel> GetTicketsByAssignmentStatus(string status);
        Object GetTicketDetails(string id);
        Attachment GetAttachmentByTicketId(string id);
        TicketAssignment GetAssignmentByTicketId(string id);
        Team GetTeamByUserId(string id);
        User GetAgentById(string id);
        IEnumerable<CategoryType> GetCategoryTypes();
        IEnumerable<StatusType> GetStatusTypes();
        IEnumerable<User> GetSupportAgents();
        IEnumerable<TicketAssignment> GetTicketAssignments();
        TicketViewModel InitializeModel(string type);
    }
}
