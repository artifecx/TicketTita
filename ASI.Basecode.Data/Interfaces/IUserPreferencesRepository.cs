using ASI.Basecode.Data.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IUserPreferencesRepository
    {
        Dictionary<string, string> GetUserPreferences(string userId);
        Task UpdateUserPreferencesAsync(string userId, Dictionary<string, string> updatedPreferences);
        Task<KeyValuePair<string, string>> FindUserPreferenceByKeyAsync(string userId, string key);
    }   
}
