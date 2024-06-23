using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    public class UserController : ControllerBase<UserController>
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService,
             IHttpContextAccessor httpContextAccessor,
                               ILoggerFactory loggerFactory,
                               IConfiguration configuration,
                               IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            var data = _userService.RetrieveAll();
            return View(data);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult Update(Guid SelectedUserId)
        {
            
            var SelectedUser = _userService.RetrieveAll().Where(s => s.UserId == SelectedUserId).FirstOrDefault();
            return View(SelectedUser);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Details(Guid SelectedUserId)
        {
            var SelectedUser = _userService.RetrieveAll().Where(s => s.UserId == SelectedUserId).FirstOrDefault();
            return View(SelectedUser);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Delete(Guid SelectedUserId)
        {
            var SelectedUser = _userService.RetrieveAll().Where(s => s.UserId == SelectedUserId).FirstOrDefault();
            return View(SelectedUser);
        }


        [HttpPost]
        [Authorize]
        public IActionResult PostCreate(UserViewModel model)
        {
            _userService.Add(model);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        public IActionResult PostUpdate(UserViewModel model)
        {
            _userService.Update(model);
             return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        public IActionResult PostDelete(Guid UserId)
        {
            _userService.Delete(UserId);
            return RedirectToAction("Index");
        }
    }
}
