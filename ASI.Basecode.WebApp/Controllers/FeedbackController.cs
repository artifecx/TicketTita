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
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    public class FeedbackController : ControllerBase<FeedbackController>
    {
        private readonly IFeedbackService _feedbackService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackController"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="feedbackService">The feedback service.</param>
        /// <param name="tokenValidationParametersFactory">The token validation parameters factory.</param>
        /// <param name="tokenProviderOptionsFactory">The token provider options factory.</param>
        public FeedbackController(
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IFeedbackService feedbackService,
            TokenValidationParametersFactory tokenValidationParametersFactory,
            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._feedbackService = feedbackService;
        }

        /// <summary>Show all feedback</summary>
        [Authorize]
        public async Task<IActionResult> ViewAllAsync(string sortOrder)
        {
            return await HandleExceptionAsync(async () =>
            {
                var feedbacks = await _feedbackService.GetAllAsync();
                if (User.IsInRole("Employee"))
                    feedbacks = feedbacks.Where(x => x.UserId == UserId);

                return View(feedbacks);
            }, "ViewAll");
        }

        #region GET Methods
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ViewFeedbackAsync(string id)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (string.IsNullOrEmpty(id)) return RedirectToAction("ViewAll");
                var feedback = await _feedbackService.GetFeedbackByTicketIdAsync(id);
                if (feedback == null) return RedirectToAction("ViewAll");
                return View(feedback);
            }, "ViewFeedback");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ProvideFeedbackAsync(string id)
        {
            return await HandleExceptionAsync(async () =>
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(userId))
                    return RedirectToAction("ViewAll");

                var feedback = await _feedbackService.InitializeModelAsync(userId, id);
                if (feedback == null) return RedirectToAction("ViewAll");

                return View(feedback);
            }, "ProvideFeedback");
        }
        #endregion GET Methods

        #region POST Methods
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ProvideFeedbackAsync(FeedbackViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.TicketId))
                    return RedirectToAction("ViewAll");

                await _feedbackService.AddAsync(model);

                TempData["CreateMessage"] = "Created Successfully";
                return RedirectToAction("ViewAll");
            }, "ProvideFeedback");
        }
        #endregion POST Methods
    }
}
