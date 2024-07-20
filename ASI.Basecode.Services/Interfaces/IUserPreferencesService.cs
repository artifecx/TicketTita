using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserPreferencesService
    {
        Task<UserPreferencesViewModel> GetUserPreferences(string userId);
        Task UpdateUserPreferences(UserPreferencesViewModel model);
        Task<KeyValuePair<string, string>> GetUserPreferenceByKey(string userId, string key);
    }
}
