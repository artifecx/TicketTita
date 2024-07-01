using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class Ticket
    {
        public Ticket()
        {
            ActivityLogs = new HashSet<ActivityLog>();
            Attachments = new HashSet<Attachment>();
            Feedbacks = new HashSet<Feedback>();
            Notifications = new HashSet<Notification>();
            TicketAssignments = new HashSet<TicketAssignment>();
        }

        public string TicketId { get; set; }
        public string IssueDescription { get; set; }
        public string CategoryTypeId { get; set; }
        public string PriorityTypeId { get; set; }
        public string StatusTypeId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string UserId { get; set; }
        public byte[] Attachment { get; set; }
        public string Subject { get; set; }
        public virtual CategoryType CategoryType { get; set; }
        public virtual PriorityType PriorityType { get; set; }
        public virtual StatusType StatusType { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<ActivityLog> ActivityLogs { get; set; }
        public virtual ICollection<Attachment> Attachments { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<TicketAssignment> TicketAssignments { get; set; }
    }
}
