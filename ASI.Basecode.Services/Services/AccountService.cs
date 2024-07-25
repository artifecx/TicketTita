using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.IO;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IAdminRepository _adminRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="mapper">The mapper.</param>
        public AccountService(IUserRepository repository, IMapper mapper, INotificationService notificationService, IAdminRepository adminRepository)
        {
            _mapper = mapper;
            _repository = repository;
            _notificationService = notificationService;
            _adminRepository = adminRepository;
        }
        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <param name="Email">The email.</param>
        /// <param name="password">The password.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public LoginResult AuthenticateUser(string Email,string password, ref User user)
        {
            user = new User();
            var passwordKey = PasswordManager.EncryptPassword(password);
            user = _repository.RetrieveAll().Where(x => x.Email == Email  &&
                                                     x.Password == passwordKey).FirstOrDefault();

            return user != null ? LoginResult.Success : LoginResult.Failed;
        }
        /// <summary>
        /// Notifies the password reset.
        /// </summary>
        /// <param name="Email">The email.</param>
        public void NotifyPasswordReset(string Email)
        {
            var Admins = _adminRepository.GetAll().ToList();

            foreach (var admin in Admins)
            {
                string message = $"User {Email} Requests Change password";
                _notificationService.AddNotification(null, message, "10", admin.AdminId, "User Change Password");
            }
        }
        /// <summary>
        /// Users the exists.
        /// </summary>
        /// <param name="Email">The email.</param>
        /// <returns></returns>
        public bool UserExists(string Email) { 
            return _repository.RetrieveAll().Any(x => x.Email == Email);
        }

    }
}
