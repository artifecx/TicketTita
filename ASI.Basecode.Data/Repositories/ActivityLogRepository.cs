using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class ActivityLogRepository : BaseRepository, IActivityLogRepository
    {

        public ActivityLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public async Task AddActivityLogAsync(ActivityLog activityLog)
        {
            await this.GetDbSet<ActivityLog>().AddAsync(activityLog);
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves all activity logs associated with a specific ticket.
        /// </summary>
        /// <param name="ticketId">The identifier of the ticket</param>
        /// <returns>A list of activity logs for the specified ticket</returns>
        public async Task<List<ActivityLog>> GetActivityLogsByTicketIdAsync(string ticketId)
        {
            // Validate the input parameter
            if (string.IsNullOrEmpty(ticketId))
            {
                throw new ArgumentException("Ticket ID cannot be null or empty.", nameof(ticketId));
            }

            // Fetch the activity logs from the database
            return await this.GetDbSet<ActivityLog>()
                             .Where(al => al.TicketId == ticketId)
                             .OrderByDescending(al => al.ActivityDate) // Optional: Order logs by date
                             .ToListAsync();
        }
    }
}
