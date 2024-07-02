using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Data.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        private readonly List<Role> _roles;
        private readonly List<Admin> _admins;
        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _roles = GetRoles().ToList();
            _admins = GetAdmins().ToList();
        }

        /// <summary>
        /// Retrieves all.
        /// </summary>
        /// <returns></returns>
        public IQueryable<User> RetrieveAll()
        {
            var users = this.GetDbSet<User>();

            foreach (User user in users)
            {
                user.Role = _roles.SingleOrDefault(r => r.RoleId == user.RoleId);
                user.CreatedByNavigation = _admins.SingleOrDefault(a => a.AdminId == user.CreatedBy);
                user.UpdatedByNavigation = _admins.SingleOrDefault(a => a.AdminId == user.UpdatedBy);
            }
            return users;
        }
        /// <summary>
        /// Adds the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        public void Add(User model)
        {
            AssignUserProperties(model);

            this.GetDbSet<User>().Add(model);
            UnitOfWork.SaveChanges();
        }
        /// <summary>
        /// Updates the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        public void Update(User model)
        {
            SetNavigation(model);
            this.GetDbSet<User>().Update(model);
            UnitOfWork.SaveChanges();
        }
        /// <summary>
        /// Deletes the specified user identifier.
        /// </summary>
        /// <param name="UserId">The user identifier.</param>
        public void Delete(string UserId)
        {
            var userToDelete = this.GetDbSet<User>().FirstOrDefault(s => s.UserId == UserId);
            if (userToDelete != null)
            {
                this.GetDbSet<User>().Remove(userToDelete);
                UnitOfWork.SaveChanges();
            }
        }

        #region Helper Methods
        /// <summary>
        /// Finds the by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public User FindById(string id)
        {
            return this.GetDbSet<User>().FirstOrDefault(x => x.UserId == id);
        }
        /// <summary>
        /// Finds the role by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public Role FindRoleById(string id)
        {
            return this.GetDbSet<Role>().FirstOrDefault(x => x.RoleId == id);
        }
        /// <summary>
        /// Finds the admin by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public Admin FindAdminById(string id)
        {
            return this.GetDbSet<Admin>().FirstOrDefault(x => x.AdminId == id);
        }
        /// <summary>
        /// Gets the roles.
        /// </summary>
        /// <returns></returns>
        public IQueryable<Role> GetRoles()
        {
            return this.GetDbSet<Role>();
        }
        /// <summary>
        /// Gets the admins.
        /// </summary>
        /// <returns></returns>
        public IQueryable<Admin> GetAdmins()
        {
            return this.GetDbSet<Admin>();
        }
        /// <summary>
        /// Assigns the user properties.
        /// </summary>
        /// <param name="user">The user.</param>
        public void AssignUserProperties(User user)
        {
            user.Role = _roles.SingleOrDefault(r => r.RoleId == user.RoleId);
            user.CreatedByNavigation = _admins.SingleOrDefault(a => a.AdminId == user.CreatedBy);
            user.UpdatedByNavigation = _admins.SingleOrDefault(a => a.AdminId == user.UpdatedBy);
        }
        /// <summary>
        /// Sets the navigation.
        /// </summary>
        /// <param name="user">The user.</param>
        public void SetNavigation(User user)
        {
            user.Role = _roles.SingleOrDefault(r => r.RoleId == user.RoleId);
            user.CreatedByNavigation = _admins.SingleOrDefault(a => a.AdminId == user.CreatedBy);
            user.UpdatedByNavigation = _admins.SingleOrDefault(a => a.AdminId == user.UpdatedBy);
        }
        #endregion
    }

}
