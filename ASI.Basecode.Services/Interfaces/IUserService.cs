using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserService
    {
        IEnumerable<UserViewModel> RetrieveAll();
        void Add(UserViewModel model);

        UserViewModel RetrieveUser(String UserId);
        void Update(UserViewModel model);

        void Delete(String UserId);
    }
}
