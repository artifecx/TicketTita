using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public IEnumerable<UserViewModel> RetrieveAll()
        {
            var data = _userRepository.RetrieveAll().Select(s => new UserViewModel
            {
                UserId = s.UserId,
                Email = s.Email,
                Name = s.Name,
                CreatedBy = s.CreatedBy,
                Password = PasswordManager.DecryptPassword(s.Password),
                RoleId = s.RoleId,
                UpdatedBy = s.UpdatedBy,
                CreatedTime = s.CreatedTime,
                UpdatedTime = s.UpdatedTime,
            });
            return data;
        }

        public UserViewModel RetrieveUser(string UserId)
        {
            var model = _userRepository.RetrieveAll().FirstOrDefault(s => s.UserId == UserId);
            if (model == null) return null;

            return new UserViewModel
            {
                UserId = model.UserId,
                Email = model.Email,
                Name = model.Name,
                CreatedBy = model.CreatedBy,
                Password = PasswordManager.DecryptPassword(model.Password),
                RoleId = model.RoleId,
                UpdatedBy = model.UpdatedBy,
                CreatedTime = model.CreatedTime,
                UpdatedTime = model.UpdatedTime
            };
        }

        public void Add(UserViewModel model)
        {
            var newModel = _mapper.Map<User>(model);
            newModel.UserId = Guid.NewGuid().ToString();
            newModel.Password = PasswordManager.EncryptPassword(newModel.Password);
            newModel.CreatedTime = DateTime.Now;
            newModel.CreatedBy = "D56F556E-50A4-4240-A0FF-9A6898B3A03B";
            newModel.UpdatedBy = null;
            newModel.UpdatedTime = null;
            _userRepository.Add(newModel);
        }

        public void Update(UserViewModel model)
        {

            var updatedModel = _mapper.Map<User>(model);
            updatedModel.UpdatedTime = DateTime.Now;
            updatedModel.UpdatedBy = "D56F556E-50A4-4240-A0FF-9A6898B3A03B";
            updatedModel.Password = PasswordManager.EncryptPassword(updatedModel.Password);
            _userRepository.Update(updatedModel);
        }
        /// <summary>
        /// Deletes the specified user identifier.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        public void Delete(string userId)
        {
            _userRepository.Delete(userId);
        }
    }
}
