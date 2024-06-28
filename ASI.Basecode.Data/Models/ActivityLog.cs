using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class ActivityLog
    {
        public string ActivityId { get; set; }
        public string TicketId { get; set; }
        public string UserId { get; set; }
        public string ActivityType { get; set; }
        public DateTime ActivityDate { get; set; }
        public string Details { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual User User { get; set; }
    }
}
