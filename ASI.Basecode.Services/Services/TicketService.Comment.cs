using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using static ASI.Basecode.Services.Exceptions.TicketExceptions;
using static ASI.Basecode.Services.Exceptions.TeamExceptions;
using ASI.Basecode.Resources.Messages;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Service class for handling operations related to tickets.
    /// </summary>
    public partial class TicketService : ITicketService
    {
        /// <summary>
        /// Adds a comment to a ticket asynchronously.
        /// </summary>
        /// <param name="model">The comment view model.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddCommentAsync(CommentViewModel model)
        {
            var comment = _mapper.Map<Comment>(model);
            var user = await _repository.UserFindByIdAsync(model.UserId);
            var ticket = await _repository.FindByIdAsync(model.TicketId);
            var parent = model.ParentId != null ? await _repository.FindCommentByIdAsync(model.ParentId) : null;

            comment.CommentId = Guid.NewGuid().ToString();
            comment.PostedDate = DateTime.Now;
            comment.User = user;
            comment.Ticket = ticket;
            comment.Parent = parent;
            ticket.UpdatedDate = DateTime.Now;

            await _repository.AddCommentAsync(comment);
            await _activityLogService.LogActivityAsync(ticket, user.UserId, Common.NewComment, string.Format(Common.NewCommentMessage, 
                (comment.Content.Length > 15 ? comment.Content.Substring(0, 15) + "..." : comment.Content)));
        }

        /// <summary>
        /// Updates an existing comment asynchronously.
        /// </summary>
        /// <param name="model">The comment view model.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="TicketException">Thrown when no changes are made to the comment content.</exception>
        public async Task UpdateCommentAsync(CommentViewModel model)
        {
            var comment = await _repository.FindCommentByIdAsync(model.CommentId);
            if (comment != null && comment.UserId == model.UserId)
            {
                if (comment.Content == model.Content)
                {
                    throw new TicketException(Errors.NoChangesReply, model.TicketId);
                }

                comment.Content = model.Content;
                comment.UpdatedDate = DateTime.Now;
                await _repository.UpdateCommentAsync(comment);
            }
        }

        /// <summary>
        /// Deletes a comment asynchronously.
        /// </summary>
        /// <param name="commentId">The comment identifier.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DeleteCommentAsync(string commentId)
        {
            await _repository.DeleteCommentAsync(commentId);
        }
    }
}
