using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class UserService: IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/> class.
        /// </summary>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="mapper">The mapper.</param>
        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UserViewModel> RetrieveAll()
        {
            var data = _userRepository.RetrieveAll().Select(s => new UserViewModel
            {
                UserId = s.UserId,
                Email = s.Email,
                Name = s.Name,
                CreatedBy = s.CreatedBy,
                Password = s.Password,
                RoleId = s.RoleId,    
                UpdatedBy = s.UpdatedBy,
                CreatedTime = s.CreatedTime,
                UpdatedTime = s.UpdatedTime,    
            });
            return data;
        }

        public UserViewModel RetrieveUser(String UserId) {
            var model = _userRepository.RetrieveAll().FirstOrDefault(s => s.UserId == UserId);
            var SelectedUser = new UserViewModel();
            SelectedUser.Email = model.Email;
            SelectedUser.CreatedTime = model.CreatedTime;
            SelectedUser.Password = model.Password;
            SelectedUser.RoleId = model.RoleId;
            SelectedUser.UpdatedBy = model.UpdatedBy;
            SelectedUser.UpdatedTime = model.UpdatedTime;
            return SelectedUser;
        }

        /// <summary>
        /// Adds the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        public void Add(UserViewModel model)
        {
            var newModel = new User();
            _mapper.Map(model, newModel);
            Guid userId = Guid.NewGuid();
            newModel.UserId = userId.ToString();
            newModel.Email = model.Email;
       
            newModel.CreatedTime = DateTime.Now;
            // newModel.CreatedBy = System.Environment.UserName;
            newModel.CreatedBy = "D56F556E-50A4-4240-A0FF-9A6898B3A03B";
            newModel.UpdatedTime = DateTime.Now;
            
            _userRepository.Add(newModel);
        }

        /// <summary>
        /// Updates the specified model.
        /// </summary>
        /// <param name="model">The model.</param>
        public void Update(UserViewModel model) {
            var SelectedUser = new User();
            _mapper.Map(model, SelectedUser);
            SelectedUser.Email = model.Email;
            SelectedUser.CreatedTime = model.CreatedTime;
            SelectedUser.Password = model.Password;
            SelectedUser.RoleId = model.RoleId;
            SelectedUser.UpdatedBy = model.UpdatedBy;
            SelectedUser.UpdatedTime = model.UpdatedTime;
            _userRepository.Update(SelectedUser);
            /*  var SelectedUser = _userRepository.RetrieveAll().Where(s => s.UserId == model.UserId).First OrDefault();
              _mapper.Map(model, SelectedUser);
              SelectedUser.UpdatedBy = System.Environment.UserName;
              SelectedUser.UpdatedTime = DateTime.Now;
              _userRepository.Update(SelectedUser);*/
        }

        /// <summary>
        /// Deletes the specified user identifier.
        /// </summary>
        /// <param name="UserId">The user identifier.</param>
        public void Delete(String UserId) {
            _userRepository.Delete(UserId);
        
        }
    }
}
