using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface ITicketRepository
    {
        IQueryable<Ticket> GetAll();
        string Add(Ticket ticket);
        void AddAttachment(Attachment attachment);
        void RemoveAttachment(Attachment attachment);
        string Update(Ticket ticket);
        void Delete(string id);
        Ticket FindById(string id);
        Attachment FindAttachmentByTicketId(string id);
        Attachment FindAttachmentById(string id);
        CategoryType FindCategoryById(string id);
        PriorityType FindPriorityById(string id);
        StatusType FindStatusById(string id);
        IQueryable<CategoryType> GetCategoryTypes();
        IQueryable<PriorityType> GetPriorityTypes();
        IQueryable<StatusType> GetStatusTypes();
    }
}
