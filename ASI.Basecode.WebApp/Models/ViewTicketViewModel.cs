using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// Create Ticket View Model
    /// </summary>
    public class ViewTicketViewModel
    {
        /// <summary>ticket id</summary>
        [JsonPropertyName("ticket_id")]
        public string Ticket_ID { get; set; }

        /// <summary>subject</summary>
        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        /// <summary>description</summary>
        [JsonPropertyName("description")]
        public string IssueDescription { get; set; }

        /// <summary>category</summary>
        [JsonPropertyName("category_name")]
        public string CategoryName { get; set; }

        /// <summary>priority</summary>
        [JsonPropertyName("priority_name")]
        public string PriorityName { get; set; }

        /// <summary>status</summary>
        [JsonPropertyName("status_name")]
        public string StatusName { get; set; }

        /// <summary>date created</summary>
        [JsonPropertyName("date_created")]
        public DateTime CreatedDate { get; set; }

        /// <summary>attachment</summary>
        [JsonPropertyName("attachment")]
        public AttachmentViewModel Attachment { get; set; }

        /// <summary>date updated</summary>
        [JsonPropertyName("date_updated")]
        public DateTime? UpdatedDate { get; set; }

        /// <summary>date resolved</summary>
        [JsonPropertyName("date_resolved")]
        public DateTime? ResolvedDate { get; set; }

        /// <summary>ticket creator</summary>
        [JsonPropertyName("user_id")]
        public string User_ID { get; set; }

        /// <summary>ticket assigned to</summary>
        [JsonPropertyName("assignment")]
        public TicketAssignmentViewModel Assignment { get; set; }


        /// <summary>temporary implementation of assignment view</summary>
        public class TicketAssignmentViewModel
        {
            public int Assignment_ID { get; set; }
            public int Team_ID { get; set; }
            public int User_ID { get; set; }
            public DateTime AssignedDate { get; set; }
        }

        /// <summary>temporary implementation, idk how/where we store attachments</summary>
        public class AttachmentViewModel
        {
            public int Attachment_ID { get; set; }
            public string FileName { get; set; }
            public string FileUrl { get; set; }
            public string FileType { get; set; }
        }
    }
}