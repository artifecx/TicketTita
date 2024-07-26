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
        /// Allows the user to update the ticket status and/or priority.
        /// </summary>
        /// <param name="model">The ticket view model.</param>
        /// <returns>A JSON result indicating success or failure.</returns>
        [HttpPost]
        [Authorize]
        [Route("updatetracking")]
        public async Task<IActionResult> UpdateTracking([FromBody] TicketViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (!string.IsNullOrEmpty(model.TicketId) &&
                    (!string.IsNullOrEmpty(model.StatusTypeId) ||
                    !string.IsNullOrEmpty(model.PriorityTypeId)))
                {
                    await _ticketService.UpdateTrackingAsync(model);
                    TempData["SuccessMessage"] = Common.SuccessUpdateTicket;
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = Errors.ErrorUpdateTicket;
                return Json(new { success = false });
            }, "UpdateStatus");
        }
    }
}
