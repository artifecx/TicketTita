using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASI.Basecode.Resources.Messages;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Service class for handling operations related to feedback.
    /// </summary>
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _repository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IActivityLogService _activityLogService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackService"/> class.
        /// </summary>
        /// <param name="repository">The feedback repository.</param>
        /// <param name="ticketRepository">The ticket repository.</param>
        /// <param name="activityLogService">The activity log service.</param>
        /// <param name="notificationService">The notification service.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="logger">The logger.</param>
        public FeedbackService(
            IFeedbackRepository repository,
            ITicketRepository ticketRepository,
            IActivityLogService activityLogService,
            INotificationService notificationService,
            IMapper mapper,
            ILogger<FeedbackService> logger)
        {
            _mapper = mapper;
            _repository = repository;
            _ticketRepository = ticketRepository;
            _activityLogService = activityLogService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Adds a ticket feedback asynchronously.
        /// </summary>
        /// <param name="feedback">The feedback view model.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
                    await _activityLogService.LogActivityAsync(ticket, user.UserId, Common.AddFeedbackLog, string.Format(Common.FeedbackCreated, newFeedback.FeedbackRating));
                    _notificationService.CreateNotification(ticket, Convert.ToInt32(Common.CreateNotificationForFeedback), null, ticket.TicketAssignment?.AgentId);
                }
            }
        }

        /// <summary>
        /// Retrieves all feedbacks asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains an enumerable of feedback view models.</returns>
        public async Task<IEnumerable<FeedbackViewModel>> GetAllAsync()
        {
            var feedbacks = await _repository.GetAllAsync();
            var feedbackViewModels = _mapper.Map<IEnumerable<FeedbackViewModel>>(feedbacks).ToList();

            return feedbackViewModels;
        }

        /// <summary>
        /// Retrieves feedback by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The feedback identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the feedback view model.</returns>
        public async Task<FeedbackViewModel> GetFeedbackByIdAsync(string id) =>
            _mapper.Map<FeedbackViewModel>(await _repository.FindFeedbackByIdAsync(id));

        /// <summary>
        /// Retrieves feedback by ticket identifier asynchronously.
        /// </summary>
        /// <param name="id">The ticket identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the feedback view model.</returns>
        public async Task<FeedbackViewModel> GetFeedbackByTicketIdAsync(string id) =>
            _mapper.Map<FeedbackViewModel>(await _repository.FindFeedbackByTicketIdAsync(id));
    }
}
