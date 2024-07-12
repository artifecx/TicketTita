using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using LinqKit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _repository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TicketService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="mapper">The mapper.</param>
        public FeedbackService(IFeedbackRepository repository,
                            ITicketRepository ticketRepository,
                            IMapper mapper, ILogger<TicketService> logger)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
            _ticketRepository = ticketRepository;
        }

        public async Task AddAsync(FeedbackViewModel feedback)
        {
            var existingFeedback = await _repository.FindFeedbackByTicketIdAsync(feedback.TicketId);
            if (existingFeedback == null)
            {
                var ticket = await _ticketRepository.FindByIdAsync(feedback.TicketId);
                var user = await _repository.FindUserByIdAsync(feedback.UserId);

                if (ticket != null && user != null)
                {
                    var newFeedback = _mapper.Map<Feedback>(feedback);
                    newFeedback.FeedbackId = Guid.NewGuid().ToString();
                    newFeedback.CreatedDate = DateTime.Now;
                    newFeedback.TicketId = feedback.TicketId;
                    newFeedback.UserId = feedback.UserId;
                    newFeedback.Ticket = ticket;
                    newFeedback.User = user;

                    ticket.Feedback = newFeedback;
                    user.Feedbacks.Add(newFeedback);

                    await _repository.AddAsync(newFeedback);
                }
                else
                {
                    LogError("AddAsync", "User/ticket does not exist.");
                }
            }
            else
            {
                LogError("AddAsync", "Feedback for ticket already exists.");
            }
        }

        public async Task<FeedbackViewModel> InitializeModelAsync(string userId, string id)
        {
            return new FeedbackViewModel
            {
                UserId = userId,
                User = await _repository.FindUserByIdAsync(userId),
                TicketId = id,
                Ticket = await _ticketRepository.FindByIdAsync(id)
            };
        }

        public async Task<IEnumerable<FeedbackViewModel>> GetAllAsync()
        {
            var feedbacks = await _repository.GetAllAsync();
            var feedbackViewModels = _mapper.Map<IEnumerable<FeedbackViewModel>>(feedbacks).ToList();

            return feedbackViewModels;
        }

        public async Task<FeedbackViewModel> GetFeedbackByTicketIdAsync(string id) =>
            _mapper.Map<FeedbackViewModel>(await _repository.FindFeedbackByTicketIdAsync(id));

        public async Task<FeedbackViewModel> GetFeedbackByIdAsync(string id) =>
            _mapper.Map<FeedbackViewModel>(await _repository.FindFeedbackByIdAsync(id));

        #region Logging methods
        /// <summary>
        /// Log the error message.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="errorMessage">The error message.</param>
        public void LogError(string methodName, string errorMessage)
        {
            _logger.LogError($"Ticket Service {methodName} : {errorMessage}");
        }
        #endregion
    }
}
