using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.ServiceModels
{
    public class NotificationViewModel
    {
        public string NotificationId { get; set; }
        public DateTime NotificationDate { get; set; }
        public string NotificationTypeId { get; set; }
        public string TicketId { get; set; }
        public bool IsRead { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public string TeamId { get; set; }
        public virtual NotificationType NotificationType { get; set; }
        public virtual Ticket Ticket { get; set; }
    }
}
