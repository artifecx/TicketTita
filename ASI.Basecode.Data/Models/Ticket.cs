using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class Ticket
    {
        public string ticket_ID { get; set; }
        public string subject { get; set; }
        public string issueDescription { get; set; }
        public int categoryType_ID { get; set; }
        public int priorityType_ID { get; set; }
        public int statusType_ID { get; set; }
        public DateTime createdDate { get; set; }
        //public Attachment attachment { get; set; }
        public DateTime? updatedDate { get; set; }
        public DateTime? resolvedDate { get; set; }
        //public Guid user_ID { get; set; }
        public int CategoryNumber { get; set; }
        public CategoryType Category { get; set; }
        public PriorityType Priority { get; set; }
        public StatusType Status { get; set; }
    }

    public class Attachment
    {
        public Guid attachment_ID { get; set; }
        /// <summary>
        /// File Name format: NN_CT_IN
        /// NN = Ticket Number
        /// CT = Content Type (file extension: JPEG, PNG, PDF, etc.)
        /// IN = Incremental Number (how many times an attachment has been uploaded for a ticket)
        /// </summary>
        public string fileName { get; set; }
        public string contentType { get; set; }
        public byte[] fileContent { get; set; }
        public DateTime uploadedDate { get; set; }
    }

    public class CategoryType
    {
        public int categoryType_ID { get; set; }
        public string categoryName { get; set; }
        public string Description { get; set; }
    }

    public class PriorityType
    {
        public int priorityType_ID { get; set; }
        public string priorityName { get; set; }
        public string Description { get; set; }
    }

    public class StatusType
    {
        public int statusType_ID { get; set; }
        public string statusName { get; set; }
        public string Description { get; set; }
    }
}
