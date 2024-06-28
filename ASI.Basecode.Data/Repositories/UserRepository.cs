using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {

        /// <summary>
        /// The selected user data
        /// </summary>
        private readonly List<User> _SelectedUserData = new List<User>();

        public UserRepository(IUnitOfWork unitOfWork): base(unitOfWork) { }

        /// <summary>
        /// Retrieves all.
        /// </summary>
        /// <returns></returns>
  /*      public IEnumerable<User> RetrieveAll()
        {
            return _SelectedUserData;
        }
*/
        public IQueryable<User> RetrieveAll() { 
            return this.GetDbSet<User>();
        }

        /// <summary>
        /// Adds the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        public void Add(User model)
        {
            this.GetDbSet<User>().Add(model);
            UnitOfWork.SaveChanges();

           /* _SelectedUserData.Add(model);*/
        }

        /// <summary>
        /// Updates the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        public void Update(User model) {
            this.GetDbSet<User>().Update(model);
            UnitOfWork.SaveChanges();


            /*     var SelectedUser = _SelectedUserData.Where(s => s.UserId == model.UserId).FirstOrDefault();
                 if (SelectedUser != null)
                 {
                     SelectedUser = model;
                 }
     */
        }
        /// <summary>
        /// Deletes the specified user identifier.
        /// </summary>
        /// <param name="UserId">The user identifier.</param>
        public void Delete(String UserId) {

            var userToDelete = this.GetDbSet<User>().FirstOrDefault(s => s.UserId == UserId);

            if (userToDelete != null) {
                this.GetDbSet<User>().Remove(userToDelete);
            }
            
      /*      var SelectedUser = _SelectedUserData.Where(s => s.UserId == UserId).FirstOrDefault();
            if (SelectedUser != null) { 
            _SelectedUserData.Remove(SelectedUser);
            }*/
        }
    }
}
