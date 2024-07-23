using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.Services.Services;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize]
    [Route("preferences")]
    public class UserPreferences : ControllerBase<UserPreferences>
    {
        private readonly IUserPreferencesService _userPreferencesService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPreferences"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="userPreferencesService">The user preferences service.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
        public UserPreferences(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IUserPreferencesService userPreferencesService,
            TokenValidationParametersFactory tokenValidationParametersFactory,
            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper, userPreferencesService)
        {
            this._userPreferencesService = userPreferencesService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetUserPreferences()
        {
            var preferences = await _userPreferencesService.GetUserPreferencesAsync(UserId);
            return View("ViewPreferences", preferences);
        }

        [HttpPost]
        [Route("UpdateUserPreferences")]
        public async Task<IActionResult> UpdateUserPreferences(UserPreferencesViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (model.UserId != null)
                {
                    await _userPreferencesService.UpdateUserPreferencesAsync(model);
                    TempData["SuccessMessage"] = "Settings updated successfullyy!";
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = "An error occurred while saving. Please try again.";
                return Json(new { success = false });
            }, "UpdateUserPreferences");
        }

        [HttpPost]
        [Route("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword(UserPreferencesViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (!string.IsNullOrEmpty(model.newPassword) && !string.IsNullOrEmpty(model.oldPassword))
                {
                    model.UserId = UserId;
                    _userPreferencesService.UpdateUserPassword(model);
                    TempData["SuccessMessage"] = "Password updated successfully!";
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = "An error occurred while saving. Please try again.";
                return Json(new { success = false });
            }, "UpdatePassword");
        }
    }
}
