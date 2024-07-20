using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    public partial class TicketController : ControllerBase<TicketController>
    {
        /// <summary>
        /// Allows the user to provide a feedback.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Success result</returns>
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

                    TempData["SuccessMessage"] = "Thank you for your feedback!";
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = "An error occurred while submitting your feedback. Please try again.";
                return Json(new { success = false });
            }, "ProvideFeedback");
        }
    }
}