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
            });
            return data;
        }

        public void Add(UserViewModel model)
        {
            var newModel = new User();
            _mapper.Map(model, newModel);
            newModel.CreatedBy = "Kent";
            newModel.CreatedDate = DateTime.Now;

            _userRepository.Add(newModel);
        }
    }
}
