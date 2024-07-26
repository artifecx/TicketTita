using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using static ASI.Basecode.Services.Exceptions.TicketExceptions;
using ASI.Basecode.Resources.Messages;
using ASI.Basecode.Resources.Views;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Service class for handling operations related to ticket attachments.
    /// </summary>
    public partial class TicketService : ITicketService
    {
        /// <summary>
        /// Adds a new attachment to a ticket asynchronously.
        /// </summary>
        /// <param name="attachment">The attachment entity.</param>
        /// <param name="ticket">The ticket entity.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddAttachmentAsync(Attachment attachment, Ticket ticket)
        {
            attachment.Ticket = ticket;
            if (attachment.Ticket != null && ticket.Attachments.Count <= 0)
            {
                ticket.Attachments.Add(attachment);
                await _repository.AddAttachmentAsync(attachment);
                await _activityLogService.LogActivityAsync(ticket, 
                    _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value, 
                    Common.AddAttachment, string.Format(Common.AttachmentAdded, attachment.Name));
            }
        }

        /// <summary>
        /// Removes an attachment by ticket identifier asynchronously.
        /// </summary>
        /// <param name="id">The ticket identifier.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RemoveAttachmentByTicketIdAsync(string id)
        {
            var attachment = await GetAttachmentByTicketIdAsync(id);
            if (attachment != null)
                await _repository.RemoveAttachmentAsync(attachment);
        }

        /// <summary>
        /// Handles the creation of a new ticket attachment asynchronously.
        /// </summary>
        /// <param name="model">The ticket view model.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="TicketException">Thrown when the file type is not allowed or the file size exceeds the limit.</exception>
        private async Task HandleAttachmentAsync(TicketViewModel model)
        {
            var allowedFileTypesString = FileValidation.AllowedFileTypes;
            var allowedFileTypes = new HashSet<string>(allowedFileTypesString.Split(','), StringComparer.OrdinalIgnoreCase);
            long maxFileSize = Convert.ToInt32(FileValidation.MaxFileSizeMB) * 1024 * 1024;

            if (model.File != null && model.File.Length > 0)
            {
                if (allowedFileTypes.Contains(model.File.ContentType) && model.File.Length <= maxFileSize)
                {
                    using (var stream = new MemoryStream())
                    {
                        await model.File.CopyToAsync(stream);
                        model.Attachment = new Attachment
                        {
                            AttachmentId = Guid.NewGuid().ToString(),
                            Name = model.File.FileName,
                            Content = stream.ToArray(),
                            Type = model.File.ContentType,
                            UploadedDate = DateTime.Now
                        };
                    }
                }
                else
                {
                    throw new TicketException(Common.FileTypeNotAllowedOrSizeExceeds, model.TicketId);
                }
            }
        }
    }
}
