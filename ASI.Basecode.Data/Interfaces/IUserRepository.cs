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
/*        IEnumerable<User> RetrieveAll();*/
        IQueryable<User> RetrieveAll();

        void Add(User model);

        void Update(User model);

        void Delete(String UserId);

    }
}
