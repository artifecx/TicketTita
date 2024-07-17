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
            Comments = new HashSet<Comment>();
            Notifications = new HashSet<Notification>();
        }

        public string TicketId { get; set; }
        public string Subject { get; set; }
        public string IssueDescription { get; set; }
        public string CategoryTypeId { get; set; }
        public string PriorityTypeId { get; set; }
        public string StatusTypeId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string UserId { get; set; }
        public bool IsDeleted { get; set; }

        public virtual CategoryType CategoryType { get; set; }
        public virtual PriorityType PriorityType { get; set; }
        public virtual StatusType StatusType { get; set; }
        public virtual User User { get; set; }
        public virtual Feedback Feedback { get; set; }
        public virtual TicketAssignment TicketAssignment { get; set; }
        public virtual ICollection<ActivityLog> ActivityLogs { get; set; }
        public virtual ICollection<Attachment> Attachments { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
