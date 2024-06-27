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
        void Add(Ticket ticket);
        void Update(Ticket ticket);
        void Delete(string id);
        Ticket FindById(string id);
        CategoryType FindCategoryById(string id);
        PriorityType FindPriorityById(string id);
        StatusType FindStatusById(string id);
        IQueryable<CategoryType> GetCategoryTypes();
        IQueryable<PriorityType> GetPriorityTypes();
        IQueryable<StatusType> GetStatusTypes();
    }
}
