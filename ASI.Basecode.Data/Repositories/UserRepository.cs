using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class UserRepository : IUserRepository
    {

        /// <summary>
        /// The selected user data
        /// </summary>
        private readonly List<User> _SelectedUserData = new List<User>();

        /// <summary>
        /// Retrieves all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<User> RetrieveAll()
        {
            return _SelectedUserData;
        }


        /// <summary>
        /// Adds the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        public void Add(User model)
        {

            _SelectedUserData.Add(model);
        }

        /// <summary>
        /// Updates the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        public void Update(User model) {
            /*var SelectedUser = _SelectedUserData.Where(s => s.UserId == model.UserId).FirstOrDefault();
            if (SelectedUser != null) {
                SelectedUser = model;
            }*/

        }
        /// <summary>
        /// Deletes the specified user identifier.
        /// </summary>
        /// <param name="UserId">The user identifier.</param>
        public void Delete(Guid UserId) {
            /*var SelectedUser = _SelectedUserData.Where(s => s.UserId == UserId).FirstOrDefault();
            if (SelectedUser != null) { 
            _SelectedUserData.Remove(SelectedUser);
            }*/
        }
    }
}
