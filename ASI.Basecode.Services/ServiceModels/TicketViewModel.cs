using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.ServiceModels
{
    /// <summary>
    /// Universal Ticket View Model for all ticket related operations
    /// </summary>
    public class TicketViewModel
    {
        /// <summary>The ticket identifier.</summary>
        [Display(Name = "Ticket ID")]
        public string TicketId { get; set; }

        /// <summary>The subject.</summary>
        [Display(Name = "Subject")]
        [StringLength(60)]
        [Required(ErrorMessage = "Subject is required.")]
        public string Subject { get; set; }

        /// <summary>The issue description.</summary>
        [Display(Name = "Description")]
        [StringLength(800)]
        [Required(ErrorMessage = "Description is required.")]
        public string IssueDescription { get; set; }

        /// <summary>a foreign key pointing to the Category table.</summary>
        [Display(Name = "Category")]
        [Required(ErrorMessage = "Category is required.")]
        public string CategoryTypeId { get; set; }

        /// <summary>a foreign key pointing to the Priority table.</summary>
        [Display(Name = "Priority")]
        [Required(ErrorMessage = "Priority is required.")]
        public string PriorityTypeId { get; set; }

        /// <summary>a foreign key pointing to the Status table.</summary>
        [Display(Name = "Status")]
        [Required(ErrorMessage = "Status is required.")]
        public string StatusTypeId { get; set; }

        /// <summary>The date the ticket was created</summary>
        [Display(Name = "Date Created")]
        public DateTime CreatedDate { get; set; }

        /// <summary>The date the ticket was updated</summary>
        [Display(Name = "Date Updated")]
        public DateTime? UpdatedDate { get; set; }

        /// <summary>The date the ticket was resolved</summary>
        [Display(Name = "Date Resolved")]
        public DateTime? ResolvedDate { get; set; }


        /// <summary>The category the ticket has</summary>
        [Display(Name = "Category")]
        public CategoryType CategoryType { get; set; }

        /// <summary>The priority the ticket has</summary>
        [Display(Name = "Priority")]
        public PriorityType PriorityType { get; set; }

        /// <summary>The status the ticket has</summary>
        [Display(Name = "Status")]
        public StatusType StatusType { get; set; }

        /// <summary>The files uploaded taken when create/edit is submitted.</summary>
        [Display(Name = "File")]
        public IFormFile File { get; set; }

        /// <summary>The file attachment.</summary>
        [Display(Name = "Attachment")]
        public Attachment Attachment { get; set; }


        /// <summary>List of tickets to populate the dropdown</summary>
        public IEnumerable<TicketViewModel> Tickets { get; set; }

        /// <summary>List of category types to populate the dropdown</summary>
        public IEnumerable<CategoryType> CategoryTypes { get; set; }

        /// <summary>List of priority types to populate the dropdown</summary>
        public IEnumerable<PriorityType> PriorityTypes { get; set; }

        /// <summary>List of status types to populate the dropdown</summary>
        public IEnumerable<StatusType> StatusTypes { get; set; }
    }
}
