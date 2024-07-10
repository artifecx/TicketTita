using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IFeedbackService
    {
        IQueryable<FeedbackViewModel> GetAll();
        FeedbackViewModel GetFeedbackById(string id);
        FeedbackViewModel GetFeedbackByTicketId(string id);
        FeedbackViewModel InitializeModel(string userId, string ticketId);
        void Add(FeedbackViewModel feedback);
    }
}
