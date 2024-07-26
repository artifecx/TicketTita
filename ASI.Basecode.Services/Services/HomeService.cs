using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASI.Basecode.Resources.Messages;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Service class for handling operations related to the home dashboard.
    /// </summary>
    public class HomeService : IHomeService
    {
        private readonly ITicketRepository _ticketRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeService"/> class.
        /// </summary>
        /// <param name="ticketRepository">The ticket repository.</param>
        /// <param name="notificationService">The notification service.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        public HomeService(
            ITicketRepository ticketRepository,
            INotificationService notificationService,
            ILogger<HomeService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _ticketRepository = ticketRepository;
        }

        /// <summary>
        /// Retrieves the dashboard data.
        /// </summary>
        /// <returns>A <see cref="DashboardViewModel"/> representing the dashboard data.</returns>
        public DashboardViewModel GetDashboardData()
        {
            var totalTickets = _ticketRepository.GetAllAndDeletedTicketsAsync().Result;
            var completedTicketStatuses = Common.CompletedTicketStatus.Split(',');
            var completedTickets = totalTickets.Where(t => completedTicketStatuses.Contains(t.StatusTypeId));
            var ticketsWithFeedbacks = totalTickets.Where(t => t.Feedback != null);

            var newTicketsCount = totalTickets.Count;
            var completedTicketsCount = completedTickets.Count();
            var openTicketsCount = totalTickets.Count(t => t.StatusTypeId == Common.OpenTicketStatus);
            var inProgressTicketsCount = totalTickets.Count(t => t.StatusTypeId == Common.InProgressTicketStatus);
            var unassignedTicketsCount = totalTickets.Count(t => t.TicketAssignment == null);
            var ticketsWithFeedbacksCount = ticketsWithFeedbacks.Count();
            var totalTicketsSoftwareCount = totalTickets.Count(t => t.CategoryTypeId == Common.SoftwareCategory);
            var totalTicketsHardwareCount = totalTickets.Count(t => t.CategoryTypeId == Common.HardwareCategory);
            var totalTicketsNetworkCount = totalTickets.Count(t => t.CategoryTypeId == Common.NetworkCategory);
            var totalTicketsAccountCount = totalTickets.Count(t => t.CategoryTypeId == Common.AccountCategory);
            var totalTicketsOtherCount = totalTickets.Count(t => t.CategoryTypeId == Common.OtherCategory);
            var feedbackRatings = ticketsWithFeedbacks.Select(t => t.Feedback.FeedbackRating).ToList();
            var averageResolutionTime = 0.0;
            var averageFeedbackRating = 0.0;

            var currentDate = DateTime.Now.Date;
            var sevenDaysAgo = currentDate.AddDays(-7);
            var ticketCreatedDates = totalTickets
                .Select(t => t.CreatedDate.Date)
                .Where(d => d >= sevenDaysAgo)
                .ToList();
            var ticketResolvedDates = totalTickets
                .Select(t => t.ResolvedDate?.Date)
                .Where(d => d.HasValue && d.Value >= sevenDaysAgo)
                .Select(d => d.Value)
                .ToList();

            if (completedTicketsCount > 0)
            {
                var resolutionTime = completedTickets
                    .Select(ticket => (ticket.ResolvedDate.Value - ticket.TicketAssignment.AssignedDate).TotalMinutes)
                    .ToList();
                averageResolutionTime = resolutionTime.Average();
            }
            if (feedbackRatings.Any())
            {
                averageFeedbackRating = feedbackRatings.Average();
            }

            var dashboardViewModel = new DashboardViewModel
            {
                OpenTickets = openTicketsCount,
                InProgressTickets = inProgressTicketsCount,
                UnassignedTickets = unassignedTicketsCount,
                NewTickets = newTicketsCount,
                CompletedTickets = completedTicketsCount,
                AverageResolutionTime = averageResolutionTime,
                AverageFeedbackRating = averageFeedbackRating,
                FeedbacksCount = ticketsWithFeedbacksCount,
                TotalTicketsSoftware = totalTicketsSoftwareCount,
                TotalTicketsHardware = totalTicketsHardwareCount,
                TotalTicketsNetwork = totalTicketsNetworkCount,
                TotalTicketsAccount = totalTicketsAccountCount,
                TotalTicketsOther = totalTicketsOtherCount,
                TicketCreatedDates = ticketCreatedDates,
                TicketResolvedDates = ticketResolvedDates,
                FeedbackRatings = feedbackRatings
            };
            return dashboardViewModel;
        }
    }
}
