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

namespace ASI.Basecode.Services.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _repository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITicketService _ticketService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<TicketService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="mapper">The mapper.</param>
        public FeedbackService(IFeedbackRepository repository,
                            ITicketService ticketService, IUserService userService,
                            ITicketRepository ticketRepository, IUserRepository userRepository,
                            IMapper mapper, ILogger<TicketService> logger)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
            _userService = userService;
            _ticketService = ticketService;
            _userRepository = userRepository;
            _ticketRepository = ticketRepository;
        }

        public IQueryable<FeedbackViewModel> GetAll()
        {
            var feedbacks = _mapper.ProjectTo<FeedbackViewModel>(_repository.GetAll());
            feedbacks.ForEach(feedback => SetNavigationProperties(feedback));

            return feedbacks.AsQueryable();
        }

        public FeedbackViewModel GetFeedbackByTicketId(string id)
        {
            var feedback = _mapper.Map<FeedbackViewModel>(_repository.FindFeedbackByTicketId(id));
            SetNavigationProperties(feedback);

            return feedback;    
        }
        public FeedbackViewModel GetFeedbackById(string id)
        {
            var feedback = _mapper.Map<FeedbackViewModel>(_repository.FindFeedbackById(id));
            SetNavigationProperties(feedback);

            return feedback;
        }

        public void SetNavigationProperties(FeedbackViewModel feedback)
        {
            feedback.User = _userService.RetrieveUser(feedback.UserId);
            feedback.Ticket = _ticketService.GetTicketByIdAsync(feedback.TicketId).Result;
        }

        public void Add(FeedbackViewModel feedback)
        {
            var existingFeedback = _repository.FindFeedbackByTicketId(feedback.TicketId);
            if(existingFeedback == null)
            {
                var ticket = _mapper.Map<Ticket>(_ticketService.GetTicketByIdAsync(feedback.TicketId).Result);
                var user = _mapper.Map<User>(_userService.RetrieveUser(feedback.UserId));

                if (ticket != null && user != null)
                {
                    var newFeedback = _mapper.Map<Feedback>(feedback);
                    newFeedback.FeedbackId = Guid.NewGuid().ToString();
                    newFeedback.CreatedDate = DateTime.Now;
                    newFeedback.TicketId = feedback.TicketId;
                    newFeedback.UserId = feedback.UserId;

                    ticket.Feedback = newFeedback;
                    user.Feedbacks.Add(newFeedback);

                    _repository.Add(newFeedback);
                }
                LogError("Add", "User/ticket does not exist.");
            }
            LogError("Add", "Feedback for ticket already exists.");
        }

        public FeedbackViewModel InitializeModel(string userId, string id)
        {
            return new FeedbackViewModel
            {
                UserId = userId,
                User = _userService.RetrieveUser(userId),
                TicketId = id,
                Ticket = _ticketService.GetTicketByIdAsync(id).Result
            };
        }

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