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
        IEnumerable<Ticket> GetAll();
        void Add(Ticket ticket);
        void Update(Ticket ticket);
        void Delete(string id);
        Ticket FindById(string id);
        CategoryType FindCategoryById(int id);
        PriorityType FindPriorityById(int id);
        StatusType FindStatusById(int id);
        IEnumerable<CategoryType> GetCategoryTypes();
        IEnumerable<PriorityType> GetPriorityTypes();
        IEnumerable<StatusType> GetStatusTypes();
    }
}
