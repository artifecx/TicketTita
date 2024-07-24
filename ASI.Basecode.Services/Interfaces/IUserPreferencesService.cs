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
        Task<UserPreferencesViewModel> GetUserPreferencesAsync(string userId);
        Task UpdateUserPreferencesAsync(UserPreferencesViewModel model);
        void UpdateUserPassword(UserPreferencesViewModel model);
        Task<KeyValuePair<string, string>> GetUserPreferenceByKeyAsync(string userId, string key);
    }
}
