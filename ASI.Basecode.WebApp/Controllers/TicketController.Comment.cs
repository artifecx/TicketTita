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
        /// Adds a comment to a ticket.
        /// </summary>
        /// <param name="model">The comment view model.</param>
        /// <returns>A JSON result indicating success or failure.</returns>
        [HttpPost]
        [Authorize]
        [Route("addcomment")]
        public async Task<IActionResult> AddComment([FromBody] CommentViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    if (model == null) return RedirectToAction("GetAll");
                    model.UserId = UserId;
                    await _ticketService.AddCommentAsync(model);
                    TempData["SuccessMessage"] = Common.SuccessCommentPosted;
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = Errors.ErrorCommentPost;
                return Json(new { success = false });
            }, "AddComment");
        }

        /// <summary>
        /// Edits an existing comment on a ticket.
        /// </summary>
        /// <param name="model">The comment view model.</param>
        /// <returns>A JSON result indicating success or failure.</returns>
        [HttpPost]
        [Authorize]
        [Route("editcomment")]
        public async Task<IActionResult> EditComment([FromBody] CommentViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    model.UserId = UserId;
                    await _ticketService.UpdateCommentAsync(model);
                    TempData["SuccessMessage"] = Common.SuccessCommentEdited;
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = Errors.ErrorCommentEdit;
                return Json(new { success = false });
            }, "EditComment");
        }

        /// <summary>
        /// Deletes a comment from a ticket.
        /// </summary>
        /// <param name="model">The comment view model.</param>
        /// <returns>A JSON result indicating success or failure.</returns>
        [HttpPost]
        [Authorize]
        [Route("deletecomment")]
        public async Task<IActionResult> DeleteComment([FromBody] CommentViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    await _ticketService.DeleteCommentAsync(model.CommentId);
                    TempData["SuccessMessage"] = Common.SuccessCommentDeleted;
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = Errors.ErrorCommentDelete;
                return Json(new { success = false });
            }, "DeleteComment");
        }
    }
}
