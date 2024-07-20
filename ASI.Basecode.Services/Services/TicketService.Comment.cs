using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static ASI.Basecode.Services.Exceptions.TicketExceptions;
using static ASI.Basecode.Services.Exceptions.TeamExceptions;

namespace ASI.Basecode.Services.Services
{
    public partial class TicketService : ITicketService
    {
        /// <summary>
        /// Adds a comment.
        /// </summary>
        /// <param name="model">The model.</param>
        public async Task AddCommentAsync(CommentViewModel model)
        {
            var comment = _mapper.Map<Comment>(model);
            var user = _repository.UserFindByIdAsync(model.UserId);
            var ticket = _repository.FindByIdAsync(model.TicketId);
            var parent = model.ParentId != null ? _repository.FindCommentByIdAsync(model.ParentId) : null;
            await Task.WhenAll(user, ticket, parent);

            comment.CommentId = Guid.NewGuid().ToString();
            comment.PostedDate = DateTime.Now;
            comment.User = await user;
            comment.Ticket = await ticket;
            comment.Parent = await parent;

            await _repository.AddCommentAsync(comment);
        }

        /// <summary>
        /// Updates a comment.
        /// </summary>
        /// <param name="model">The model.</param>
        public async Task UpdateCommentAsync(CommentViewModel model)
        {
            var comment = await _repository.FindCommentByIdAsync(model.CommentId);
            if (comment != null && comment.UserId == model.UserId)
            {
                if (comment.Content == model.Content)
                {
                    throw new TicketException("No changes were made to the reply.", model.TicketId);
                }

                comment.Content = model.Content;
                comment.UpdatedDate = DateTime.Now;
                await _repository.UpdateCommentAsync(comment);
            }
        }

        /// <summary>
        /// Deletes a comment.
        /// </summary>
        /// <param name="commentId">The comment identifier.</param>
        public async Task DeleteCommentAsync(string commentId)
        {
            await _repository.DeleteCommentAsync(commentId);
        }
    }
}
