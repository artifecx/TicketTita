using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ASI.Basecode.Resources.Messages;

namespace ASI.Basecode.WebApp.Controllers
{
    public partial class TicketController : ControllerBase<TicketController>
    {
        /// <summary>
        /// Allows the user to provide feedback on a ticket.
        /// </summary>
        /// <param name="model">The feedback view model.</param>
        /// <returns>A JSON result indicating success or failure.</returns>
        [HttpPost]
        [Authorize(Policy = "Employee")]
        [Route("providefeedback")]
        public async Task<IActionResult> ProvideFeedback(FeedbackViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(model.UserId) || string.IsNullOrEmpty(model.TicketId))
                        return RedirectToAction("GetAll");

                    await _feedbackService.AddAsync(model);

                    TempData["SuccessMessage"] = Common.SuccessFeedbackSubmitted;
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = Errors.ErrorFeedbackSubmission;
                return Json(new { success = false });
            }, "ProvideFeedback");
        }
    }
}
