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
        /// <param name="model">The ticket view model containing the assignment details.</param>
        /// <returns>A JSON result indicating success or failure.</returns>
        [HttpPost]
        [Authorize]
        [Route("updateassignment")]
        public async Task<IActionResult> UpdateAssignment([FromBody] TicketViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (!string.IsNullOrEmpty(model.TicketId) && !string.IsNullOrEmpty(model.AgentId))
                {
                    var status = await _ticketService.UpdateAssignmentAsync(model);
                    TempData["SuccessMessage"] = string.Format(Common.SuccessTicketAssignment, status);
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = Errors.ErrorTicketAssignment;
                return Json(new { success = false });
            }, "UpdateAssignment");
        }
    }
}
