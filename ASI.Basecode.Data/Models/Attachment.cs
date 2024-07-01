using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class Attachment
    {
        public string AttachmentId { get; set; }
        public string TicketId { get; set; }
        public string Name { get; set; }
        public byte[] Content { get; set; }
        public string Type { get; set; }
        public DateTime UploadedDate { get; set; }

        public virtual Ticket Ticket { get; set; }
    }
}
