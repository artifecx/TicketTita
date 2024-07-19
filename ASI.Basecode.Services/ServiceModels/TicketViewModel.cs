using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ASI.Basecode.Services.ServiceModels
{
    /// <summary>
    /// Universal Ticket View Model for all ticket related operations
    /// </summary>
    public class TicketViewModel
    {
        #region Ticket Properties
        /// <summary>The ticket identifier.</summary>
        [Display(Name = "Ticket ID")]
        public string TicketId { get; set; }

        /// <summary>The ticket subject.</summary>
        [Display(Name = "Subject")]
        [StringLength(100)]
        [Required(ErrorMessage = "Subject is required.")]
        public string Subject { get; set; }

        /// <summary>The ticket issue description.</summary>
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

        /// <summary>The user identifier for who submitted the ticket.</summary>
        [Display(Name = "Submitted by")]
        public string UserId { get; set; }
        #endregion


        #region Relationships
        public string AgentId { get; set; }
        public string TeamId { get; set; }

        /// <summary>The agent assigned to the ticket.</summary>
        [Display(Name = "Agent")]
        public User Agent { get; set; }

        /// <summary>The team assigned to the ticket.</summary>
        [Display(Name = "Team")]
        public Team Team { get; set; }

        /// <summary>Holds the ticket assignment values.</summary>
        [Display(Name = "Ticket Assignee")]
        public TicketAssignment TicketAssignment { get; set; }

        /// <summary>The file attachment.</summary>
        [Display(Name = "Attachment")]
        public Attachment Attachment { get; set; }

        /// <summary>The file uploaded taken when create/edit is submitted.</summary>
        [Display(Name = "File")]
        public IFormFile File { get; set; }

        [Display(Name = "Feedback")]
        public Feedback Feedback { get; set; }

        [Display(Name = "Comments")]
        public IEnumerable<CommentViewModel> Comments { get; set; }
        #endregion


        #region Navigation Properties for Foreign Keys
        [Display(Name = "Category")]
        public CategoryType CategoryType { get; set; }

        [Display(Name = "Priority")]
        public PriorityType PriorityType { get; set; }

        [Display(Name = "Status")]
        public StatusType StatusType { get; set; }

        [Display(Name = "User")]
        public User User { get; set; }
        #endregion


        #region Dropdown Population
        [Display(Name = "Teams")]
        public IEnumerable<Team> Teams { get; set; }
        [Display(Name = "Support Agents")]
        public IEnumerable<User> Agents { get; set; }
        public IEnumerable<User> AgentsWithNoTeam { get; set; }

        [Display(Name = "Users")]
        public IEnumerable<User> Users { get; set; }

        public IEnumerable<TicketViewModel> Tickets { get; set; }

        public IEnumerable<CategoryType> CategoryTypes { get; set; }

        public IEnumerable<PriorityType> PriorityTypes { get; set; }

        public IEnumerable<StatusType> StatusTypes { get; set; }
        #endregion
    }
}
