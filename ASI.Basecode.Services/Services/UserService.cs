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
                Password = s.Password,
                Role = s.Role,
                UpdatedBy = s.UpdatedBy,
                CreatedTime = s.CreatedTime,
                UpdatedTime = s.UpdatedTime,    
            });
            return data;
        }

        public void Add(UserViewModel model)
        {
            var newModel = new User();
            _mapper.Map(model, newModel);
            newModel.UserId = Guid.NewGuid();
            newModel.CreatedTime = DateTime.Now;
            newModel.CreatedBy = System.Environment.UserName;
            newModel.UpdatedTime = DateTime.Now;
            _userRepository.Add(newModel);
        }

        public void Update(UserViewModel model) {
            var SelectedUser = _userRepository.RetrieveAll().Where(s => s.UserId == model.UserId).FirstOrDefault();
            _mapper.Map(model, SelectedUser);
            SelectedUser.UpdatedBy = System.Environment.UserName;
            SelectedUser.UpdatedTime = DateTime.Now;
            _userRepository.Update(SelectedUser);
        }

        public void Delete(Guid UserId) {
            _userRepository.Delete(UserId);
        
        }

    }
}
