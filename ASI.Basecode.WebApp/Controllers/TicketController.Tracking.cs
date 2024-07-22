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
        /// Allows the user to update the ticket status and/or priority
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Success or fail status</returns>
        [HttpPost]
        [Authorize]
        [Route("updatetracking")]
        public async Task<IActionResult> UpdateTracking([FromBody] TicketViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if(!string.IsNullOrEmpty(model.TicketId) && 
                    (!string.IsNullOrEmpty(model.StatusTypeId) || 
                    !string.IsNullOrEmpty(model.PriorityTypeId)))
                {
                    await _ticketService.UpdateTrackingAsync(model);
                    TempData["SuccessMessage"] = "Ticket updated successfully!";
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = "An error occurred while updating the ticket. Please try again.";
                return Json(new { success = false });
            }, "UpdateStatus");
        }
    }
}