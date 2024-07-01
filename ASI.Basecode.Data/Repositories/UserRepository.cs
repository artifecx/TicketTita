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

        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            _roles = GetRoles().ToList();
            _admins = GetAdmins().ToList();
        }

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

        public void Add(User model)
        {
            AssignUserProperties(model);

            this.GetDbSet<User>().Add(model);
            UnitOfWork.SaveChanges();
        }

        public void Update(User model)
        {
            SetNavigation(model);
            this.GetDbSet<User>().Update(model);
            UnitOfWork.SaveChanges();
        }

        public void Delete(string UserId)
        {
            var userToDelete = this.GetDbSet<User>().FirstOrDefault(s => s.UserId == UserId);
            if (userToDelete != null)
            {
                this.GetDbSet<User>().Remove(userToDelete);
                UnitOfWork.SaveChanges();
            }
        }

        public User FindById(string id)
        {
            return this.GetDbSet<User>().FirstOrDefault(x => x.UserId == id);
        }

        public Role FindRoleById(string id)
        {
            return this.GetDbSet<Role>().FirstOrDefault(x => x.RoleId == id);
        }

        public Admin FindAdminById(string id)
        {
            return this.GetDbSet<Admin>().FirstOrDefault(x => x.AdminId == id);
        }

        public IQueryable<Role> GetRoles()
        {
            return this.GetDbSet<Role>();
        }

        public IQueryable<Admin> GetAdmins()
        {
            return this.GetDbSet<Admin>();
        }

        public void AssignUserProperties(User user)
        {
            user.Role = _roles.SingleOrDefault(r => r.RoleId == user.RoleId);
            user.CreatedByNavigation = _admins.SingleOrDefault(a => a.AdminId == user.CreatedBy);
            user.UpdatedByNavigation = _admins.SingleOrDefault(a => a.AdminId == user.UpdatedBy);
        }

        public void SetNavigation(User user)
        {
            user.Role = _roles.SingleOrDefault(r => r.RoleId == user.RoleId);
            user.CreatedByNavigation = _admins.SingleOrDefault(a => a.AdminId == user.CreatedBy);
            user.UpdatedByNavigation = _admins.SingleOrDefault(a => a.AdminId == user.UpdatedBy);
        }
    }
}
