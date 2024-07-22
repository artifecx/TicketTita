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
            var preferences = await _userPreferencesService.GetUserPreferences(UserId);
            return View("ViewPreferences", preferences);
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> UpdateUserPreferences(UserPreferencesViewModel model)
        {
            await _userPreferencesService.UpdateUserPreferences(model);
            return RedirectToAction("GetUserPreferences");
        }
    }
}
