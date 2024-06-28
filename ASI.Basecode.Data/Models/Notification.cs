using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class Notification
    {
        public string NotificationId { get; set; }
        public DateTime NotificationDate { get; set; }
        public string NotificationTypeId { get; set; }
        public string TicketId { get; set; }

        public virtual NotificationType NotificationType { get; set; }
        public virtual Ticket Ticket { get; set; }
    }
}
