using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class Comment
    {
        public string CommentId { get; set; }
        public string UserId { get; set; }
        public string TicketId { get; set; }
        public string ParentId { get; set; }
        public string Content { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public virtual Comment Parent { get; set; }
        public virtual Ticket Ticket { get; set; }
        public virtual User User { get; set; }
        public virtual Comment InverseParent { get; set; }
    }
}
