using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class TicketAssignment
    {
        public string AssignmentId { get; set; }
        public string TeamId { get; set; }
        public string AgentId { get; set; }
        public string TicketId { get; set; }
        public DateTime AssignedDate { get; set; }
        public string AssignedById { get; set; }

        public virtual User Agent { get; set; }
        public virtual User AssignedBy { get; set; }
        public virtual Team Team { get; set; }
        public virtual Ticket Ticket { get; set; }
    }
}
