using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    public class UserRepository: IUserRepository
    {
        private readonly List<User> _data = new List<User>();
        private int _nextId = 1;

        public IEnumerable<User> RetrieveAll()
        {
            return _data;
        }

        public void Add(User model)
        {
            model.UserId = _nextId++;
            _data.Add(model);
        }
    }
}
