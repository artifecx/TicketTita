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

        #region GET methods      
        
        /// <summary>
        /// Transfers the user to the Create Screen.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]

        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Updates the specified selected user identifier.
        /// </summary>
        /// <param name="SelectedUserId">The selected user identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public IActionResult Update(String SelectedUserId)
        {
            
            var SelectedUser = _userService.RetrieveAll().Where(s => s.UserId == SelectedUserId).FirstOrDefault();
            return View(SelectedUser);
        }

        /// <summary>
        /// Detailses the specified selected user identifier.
        /// </summary>
        /// <param name="SelectedUserId">The selected user identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public IActionResult Details(String SelectedUserId)
        {
            var SelectedUser = _userService.RetrieveUser(SelectedUserId);
            return View(SelectedUser);
        }

        /// <summary>
        /// Deletes the specified selected user identifier.
        /// </summary>
        /// <param name="SelectedUserId">The selected user identifier.</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        public IActionResult Delete(String SelectedUserId)
        {
            var SelectedUser = _userService.RetrieveAll().Where(s => s.UserId == SelectedUserId).FirstOrDefault();
            return View(SelectedUser);
        }

        #endregion

        #region POST Methods        

        /// <summary>
        /// Posts the create.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public IActionResult PostCreate(UserViewModel model)
        {
            _userService.Add(model);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Posts the update.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public IActionResult PostUpdate(UserViewModel model)
        {
            _userService.Update(model);
             return RedirectToAction("Index");
        }

        /// <summary>
        /// Posts the delete.
        /// </summary>
        /// <param name="UserId">The user identifier.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public IActionResult PostDelete(String UserId)
        {
            _userService.Delete(UserId);
            return RedirectToAction("Index");
        }

        #endregion
    }
}
