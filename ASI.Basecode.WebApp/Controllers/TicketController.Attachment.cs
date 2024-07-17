using ASI.Basecode.WebApp.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    public partial class TicketController : ControllerBase<TicketController>
    {
        /// <summary>
        /// Allows the user to download the attachment
        /// </summary>
        /// <param name="id">Attachment identifier</param>
        /// <returns>The file to download</returns>
        [HttpGet]
        [Authorize]
        public async Task<FileResult> DownloadAttachment(string id)
        {
            return await HandleExceptionAsync(async () =>
            {
                if (string.IsNullOrEmpty(id)) return null;
                var attachment = await _ticketService.GetAttachmentByTicketIdAsync(id);
                if (attachment == null) return null;
                return File(attachment.Content, "application/octet-stream", attachment.Name);
            }, "DownloadAttachment");
        }
    }
}