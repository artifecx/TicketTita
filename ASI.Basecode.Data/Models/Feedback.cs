using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class Feedback
    {
        public string FeedbackId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int FeedbackRating { get; set; }
        public string FeedbackContent { get; set; }
        public string TicketId { get; set; }
        public string UserId { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual User User { get; set; }
    }
}
