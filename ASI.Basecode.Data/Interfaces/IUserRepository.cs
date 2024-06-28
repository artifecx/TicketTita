using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IUserRepository
    {
        IQueryable<User> RetrieveAll();
        void Add(User model);
        void Update(User model);
        void Delete(String UserId);
        User FindById(string id);
        Role FindRoleById(string id);
        Admin FindAdminById(string id);
        IQueryable<Role> GetRoles();

        IQueryable<Admin> GetAdmins();
        void AssignUserProperties(User user);
        void SetNavigation(User user);

    }
}
