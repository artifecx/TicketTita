using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ASI.Basecode.WebApp.Models
{
    /// <summary>
    /// Create Ticket View Model
    /// </summary>
    public class EditTicketViewModel
    {
        /// <summary>subject</summary>
        [UIHint("SingleLineText")]
        [StringLength(60)]
        [JsonPropertyName("subject")]
        [Required(ErrorMessage = "Subject is required.")]
        public string Subject { get; set; }

        /// <summary>description</summary>
        [UIHint("MultiLineText")]
        [StringLength(800)]
        [JsonPropertyName("description")]
        [Required(ErrorMessage = "Description is required.")]
        public string IssueDescription { get; set; }

        /// <summary>category</summary>
        [UIHint("DropDownList")]
        [JsonPropertyName("category")]
        [Required(ErrorMessage = "Category is required.")]
        public int CategoryType_ID { get; set; }

        /// <summary>priority</summary>
        [UIHint("DropDownList")]
        [JsonPropertyName("priority")]
        [Required(ErrorMessage = "Priority is required.")]
        public int PriorityType_ID { get; set; }

        /// <summary>status</summary>
        [UIHint("DropDownList")]
        [JsonPropertyName("status")]
        [Required(ErrorMessage = "Status is required.")]
        public int StatusType_ID { get; set; }

        /// <summary>existing attachments</summary>
        [JsonPropertyName("attachment")]
        public AttachmentViewModel ExistingAttachment { get; set; }

        /// <summary>attachment</summary>
        [UIHint("FileUpload")]
        [JsonPropertyName("new_attachment")]
        public IFormFile NewAttachment { get; set; }


        /// <summary>temporary implementation, move to a separate file, change structure</summary>
        public class AttachmentViewModel
        {
            public int AttachmentId { get; set; }
            public string FileName { get; set; }
            public string FileUrl { get; set; }
            public string FileType { get; set; }
        }
    }
}