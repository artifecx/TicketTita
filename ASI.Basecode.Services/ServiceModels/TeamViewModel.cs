using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    /// <summary>
    /// Team view model
    /// </summary>
    public class TeamViewModel
    {
        #region Team Properties
        [Display(Name = "Team ID")]
        public string TeamId { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessage = "Team name is required.")]
        public string Name { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Specialization")]
        public string SpecializationId { get; set; }
        #endregion

        #region Team Analytics
        [Display(Name = "Number of Agents")]
        public string NumberOfAgents { get; set; }
        [Display(Name = "Active Tickets")]
        public string ActiveTicketsCount { get; set; }
        [Display(Name = "Completed Tickets")]
        public string CompletedTicketsCount { get; set; }
        [Display(Name = "Average Resolution Time")]
        public string AverageResolutionTime { get; set; }
        [Display(Name = "Average Feedback Rating")]
        public string AverageFeedbackRating { get; set; }
        #endregion

        #region Team Navigation Properties
        [Display(Name = "Number of Agents")]
        public IEnumerable<TeamMember> TeamMembers { get; set; }
        public CategoryType Specialization { get; set; }
        public IEnumerable<TicketAssignment> TicketAssignments { get; set; }
        #endregion

        #region Dropdown Population
        public IEnumerable<User> Agents { get; set; }
        public IEnumerable<Team> Teams { get; set; }
        #endregion
    }
}
