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
    public class UserPreferencesRepository : BaseRepository, IUserPreferencesRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public UserPreferencesRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public Dictionary<string, string> GetUserPreferences(string userId)
        {
            var preferences = this.GetDbSet<User>().Where(x => x.UserId == userId).Select(x => x.Preferences).FirstOrDefault();
            if(preferences == null)
            {
                return new Dictionary<string, string>();
            }
            return JsonSerializer.Deserialize<Dictionary<string, string>>(preferences);
        }

        public async Task UpdateUserPreferences(string userId, Dictionary<string, string> updatedPreferences)
        {
            var user = await this.GetDbSet<User>().FirstOrDefaultAsync(x => x.UserId == userId);
            if(user != null)
            {
                user.Preferences = JsonSerializer.Serialize(updatedPreferences);
                this.GetDbSet<User>().Update(user);
                await UnitOfWork.SaveChangesAsync();
            }
        }

        public async Task<KeyValuePair<string,string>> FindUserPreferenceByKey(string userId, string key)
        {
            return GetUserPreferences(userId).FirstOrDefault(x => x.Key == key);
        }
    }
}
