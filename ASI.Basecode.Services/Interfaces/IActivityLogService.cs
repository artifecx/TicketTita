using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IActivityLogService
    {
        Task LogActivityAsync(Ticket ticket, string userId, string activityType, string details);
        Task<IEnumerable<ActivityLog>> GetActivityLogsByTicketIdAsync(string ticketId);
    }
}
