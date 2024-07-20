using ASI.Basecode.Services.ServiceModels;
using ASI.Basecode.WebApp.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    public partial class TicketController : ControllerBase<TicketController>
    {
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment([FromBody] CommentViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    if (model == null) return RedirectToAction("GetAll");
                    model.UserId = UserId;
                    await _ticketService.AddCommentAsync(model);
                    TempData["SuccessMessage"] = "Comment posted successfully!";
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = "An error occurred while posting your comment. Please try again.";
                return Json(new { success = false });
            }, "AddComment");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditComment([FromBody] CommentViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    model.UserId = UserId;
                    await _ticketService.UpdateCommentAsync(model);
                    TempData["SuccessMessage"] = "Comment edited successfully!";
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = "An error occurred while editing your comment. Please try again.";
                return Json(new { success = false });
            }, "EditComment");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteComment([FromBody] CommentViewModel model)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    await _ticketService.DeleteCommentAsync(model.CommentId);
                    TempData["SuccessMessage"] = "Comment deleted successfully!";
                    return Json(new { success = true });
                }
                TempData["ErrorMessage"] = "An error occurred while deleting your comment. Please try again.";
                return Json(new { success = false });
            }, "DeleteComment");
        }
    }
}