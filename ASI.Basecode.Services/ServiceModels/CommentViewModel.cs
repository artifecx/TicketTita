using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ASI.Basecode.Services.ServiceModels
{
    /// <summary>
    /// Comment View Model
    /// </summary>
    public class CommentViewModel
    {
        public string CommentId { get; set; }
        public string UserId { get; set; }
        public string TicketId { get; set; }
        public string ParentId { get; set; }

        [Required(ErrorMessage = "Content cannot be empty.")]
        [StringLength(500, ErrorMessage = "Content cannot be more than 500 characters.")]
        public string Content { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public virtual Comment Parent { get; set; }
        public virtual Ticket Ticket { get; set; }
        public virtual User User { get; set; }
        public virtual IEnumerable<CommentViewModel> Replies { get; set; }
    }
}
