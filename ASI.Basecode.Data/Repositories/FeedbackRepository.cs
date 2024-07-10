using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace ASI.Basecode.Data.Repositories
{
    public class FeedbackRepository : BaseRepository, IFeedbackRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TicketRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public FeedbackRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        /// <summary>
        /// Gets all feedbacks with includes.
        /// </summary>
        /// <returns>A list of all tickets Ticket (IQueryable)</returns>
        private IQueryable<Feedback> GetFeedbacksWithIncludes()
        {
            return this.GetDbSet<Feedback>().Include(f => f.Ticket)
                                            .Include(f => f.User);
        }

        public IQueryable<Feedback> GetAll()
        {
            return GetFeedbacksWithIncludes();
        }

        public Feedback FindFeedbackById(string id)
        {
            return GetFeedbacksWithIncludes().FirstOrDefault(x => x.FeedbackId == id);
        }

        public Feedback FindFeedbackByTicketId(string id)
        {
            return GetFeedbacksWithIncludes().FirstOrDefault(x => x.TicketId == id);
        }

        public void Add(Feedback feedback)
        {
            this.GetDbSet<Feedback>().Add(feedback);
            UnitOfWork.SaveChanges();
        }

        public void Delete(Feedback feedback)
        {
            this.GetDbSet<Feedback>().Remove(feedback);
            UnitOfWork.SaveChanges();
        }
    }
}