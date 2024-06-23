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
        private readonly List<User> _SelectedUserData = new List<User>();

        public IEnumerable<User> RetrieveAll()
        {
            return _SelectedUserData;
        }

        public void Add(User model)
        {

            _SelectedUserData.Add(model);
        }

        public void Update(User model) {
            var SelectedUser = _SelectedUserData.Where(s => s.UserId == model.UserId).FirstOrDefault();
            if (SelectedUser != null) {
                SelectedUser = model;
            }

        }
        public void Delete(Guid UserId) {
            var SelectedUser = _SelectedUserData.Where(s => s.UserId == UserId).FirstOrDefault();
            if (SelectedUser != null) { 
            _SelectedUserData.Remove(SelectedUser);
            }
        }
    }
}
