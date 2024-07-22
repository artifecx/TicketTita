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
    public class UserPreferencesViewModel
    {
        public string UserId { get; set; }
        public Dictionary<string, string> Preferences { get; set; }


        #region Dropdown population
        public IEnumerable<CategoryType> CategoryTypes { get; set; }
        public IEnumerable<StatusType> StatusTypes { get; set; }
        public IEnumerable<PriorityType> PriorityTypes { get; set; }
        #endregion
    }
}
