using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Authentication;
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ASI.Basecode.Resources.Messages;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Controller for handling user preferences.
    /// </summary>
    [Authorize]
    [Route("preferences")]
    public class UserPreferencesController : ControllerBase<UserPreferencesController>
    {
        private readonly IUserPreferencesService _userPreferencesService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserPreferencesController"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="userPreferencesService">The user preferences service.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
        public UserPreferencesController(
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

        /// <summary>
        /// Gets the user preferences.
        /// </summary>
        /// <returns>The user preferences view.</returns>
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetUserPreferences()
        {
            var preferences = await _userPreferencesService.GetUserPreferencesAsync(UserId);
            return View("ViewPreferences", preferences);
        }

        /// <summary>
        /// Updates the user preferences.
        /// </summary>
        /// <param name="model">The user preferences view model.</param>
        /// <returns>A JSON result indicating success or failure.</returns>
        [HttpPost]
        [Route("UpdateUserPreferences")]
        public async Task<IActionResult> UpdateUserPreferences(UserPreferencesViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (model.UserId != null)
                {
                    await _userPreferencesService.UpdateUserPreferencesAsync(model);
                    TempData["SuccessMessage"] = Common.SuccessUpdatePreferences;
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = Errors.ErrorUpdatePreferences;
                return Json(new { success = false });
            }, "UpdateUserPreferences");
        }

        /// <summary>
        /// Updates the user password.
        /// </summary>
        /// <param name="model">The user preferences view model.</param>
        /// <returns>A JSON result indicating success or failure.</returns>
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
                    TempData["SuccessMessage"] = Common.SuccessUpdatePassword;
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = Errors.ErrorUpdatePassword;
                return Json(new { success = false });
            }, "UpdatePassword");
        }
    }
}
