using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IAccountService
    {
        LoginResult AuthenticateUser(string email, string password, ref User user);
        /*void AddUser(AccountServiceModel model);*/
        void NotifyPasswordReset(string Email);
        bool UserExists(string Email);
    }
}
