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

namespace ASI.Basecode.WebApp.Controllers
{
    public class FeedbackController : ControllerBase<FeedbackController>
    {
        private readonly IFeedbackService _feedbackService;
        private readonly ITicketService _ticketService;
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketController"/> class.
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
                            ITicketService ticketService,
                            IUserService userService,
                            TokenValidationParametersFactory tokenValidationParametersFactory,
                            TokenProviderOptionsFactory tokenProviderOptionsFactory) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            this._feedbackService = feedbackService;
            this._ticketService = ticketService;
            this._userService = userService;
        }

        /// <summary>Show all feedback</summary>
        [Authorize]
        public IActionResult ViewAll(string sortOrder)
        {
            return HandleException(() =>
            {
                var feedbacks = _feedbackService.GetAll();
                if (User.IsInRole("Employee"))
                    feedbacks = feedbacks.Where(x => x.UserId == UserId);

                return View(feedbacks);
            }, "ViewAll");
        }

        #region GET Methods
        [Authorize]
        [HttpGet]
        public IActionResult ViewFeedback(string id)
        {
            return HandleException(() =>
            {
                if (string.IsNullOrEmpty(id)) return RedirectToAction("ViewAll");
                var feedback = _feedbackService.GetFeedbackByTicketId(id);
                if (feedback == null) return RedirectToAction("ViewAll");
                return View(feedback);
            }, "ViewFeedback");
        }

        [Authorize]
        [HttpGet]
        public IActionResult ProvideFeedback(string id)
        {
            return HandleException(() =>
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(userId)) 
                    return RedirectToAction("ViewAll");

                var feedback = _feedbackService.InitializeModel(userId, id);
                if (feedback == null) return RedirectToAction("ViewAll");

                return View(feedback);
            }, "ProvideFeedback");
        }
        #endregion GET Methods

        #region POST Methods
        [HttpPost]
        [Authorize]
        public IActionResult ProvideFeedback(FeedbackViewModel model)
        {
            return HandleException(() =>
            {
                if (string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.TicketId))
                    return RedirectToAction("ViewAll");

                _feedbackService.Add(model);

                TempData["CreateMessage"] = "Created Successfully";
                return RedirectToAction("ViewAll");
            }, "ProvideFeedback");
        }
        #endregion POST Methods
    }
}