using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    /// <summary>
    /// Repository class for handling operations related to user preferences.
    /// </summary>
    public class UserPreferencesRepository : BaseRepository, IUserPreferencesRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserPreferencesRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public UserPreferencesRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        /// <summary>
        /// Retrieves the user preferences for a specified user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A dictionary containing the user preferences.</returns>
        public Dictionary<string, string> GetUserPreferences(string userId)
        {
            var preferences = this.GetDbSet<User>()
                                  .Where(x => x.UserId == userId)
                                  .Select(x => x.Preferences)
                                  .FirstOrDefault();
            if (preferences == null)
            {
                return new Dictionary<string, string>();
            }
            return JsonSerializer.Deserialize<Dictionary<string, string>>(preferences);
        }

        /// <summary>
        /// Updates the user preferences asynchronously.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="updatedPreferences">The updated preferences.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdateUserPreferencesAsync(string userId, Dictionary<string, string> updatedPreferences)
        {
            var user = await this.GetDbSet<User>().FirstOrDefaultAsync(x => x.UserId == userId);
            if (user != null)
            {
                user.Preferences = JsonSerializer.Serialize(updatedPreferences);
                this.GetDbSet<User>().Update(user);
                await UnitOfWork.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Finds a specific user preference by key asynchronously.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="key">The preference key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains a key-value pair representing the preference.</returns>
        public async Task<KeyValuePair<string, string>> FindUserPreferenceByKeyAsync(string userId, string key) =>
            GetUserPreferences(userId).FirstOrDefault(x => x.Key == key);
    }
}
