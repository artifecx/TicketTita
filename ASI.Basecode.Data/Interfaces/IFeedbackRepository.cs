using ASI.Basecode.Data.Models;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IFeedbackRepository
    {
        IQueryable<Feedback> GetAll();
        Feedback FindFeedbackById(string id);
        Feedback FindFeedbackByTicketId(string id);
        void Add(Feedback feedback);
        void Delete(Feedback feedback);
    }
}
