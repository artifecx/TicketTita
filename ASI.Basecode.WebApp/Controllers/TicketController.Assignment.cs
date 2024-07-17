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
        public async Task<IActionResult> UpdateAssignment([FromBody] TicketViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (!string.IsNullOrEmpty(model.TicketId) && !string.IsNullOrEmpty(model.AgentId))
                {
                    var status = await _ticketService.UpdateAssignmentAsync(model);
                    TempData["SuccessMessage"] = $"Ticket {status}ed successfully!";
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = "An error occurred while updating the ticket assignment. Please try again.";
                return Json(new { success = false });
            }, "UpdateAssignment");
        }
    }
}