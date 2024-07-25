using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ASI.Basecode.Services.Exceptions.TeamExceptions;

namespace ASI.Basecode.Services.Services
{
    public class HomeService : IHomeService
    {
        private readonly ITicketRepository _ticketRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamService"/> class.
        /// </summary>
        /// <param name="repository">The repository</param>
        /// <param name="mapper">The mapper</param>
        /// <param name="logger">The logger</param>
        /// <param name="httpContextAccessor">The HTTP context accessor</param>
        public HomeService(
            ITicketRepository ticketRepository,
            INotificationService notificationService,
            ILogger<HomeService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _ticketRepository = ticketRepository;
        }

        public DashboardViewModel GetDashboardData()
        {
            var totalTickets = _ticketRepository.GetAllAndDeletedTicketsAsync().Result;
            var completedTickets = totalTickets.Where(t => t.StatusTypeId == "S3" || t.StatusTypeId == "S4");
            var ticketsWithFeedsbacks = totalTickets.Where(t => t.Feedback != null);

            var newTicketsCount = totalTickets.Count;
            var completedTicketsCount = completedTickets.Count();
            var openTicketsCount = totalTickets.Count(t => t.StatusTypeId == "S1");
            var inProgressTicketsCount = totalTickets.Count(t => t.StatusTypeId == "S2");
            var unassignedTicketsCount = totalTickets.Count(t => t.TicketAssignment == null);
            var ticketsWithFeedbacksCount = ticketsWithFeedsbacks.Count();
            var totalTicketsSoftwareCount = totalTickets.Count(t => t.CategoryTypeId == "C1");
            var totalTicketsHardwareCount = totalTickets.Count(t => t.CategoryTypeId == "C2");
            var totalTicketsNetworkCount = totalTickets.Count(t => t.CategoryTypeId == "C3");
            var totalTicketsAccountCount = totalTickets.Count(t => t.CategoryTypeId == "C4");
            var totalTicketsOtherCount = totalTickets.Count(t => t.CategoryTypeId == "C5");
            var feedbackRatings = ticketsWithFeedsbacks.Select(t => t.Feedback.FeedbackRating).ToList();
            var averageResolutionTime = 0.0;
            var averageFeedbackRating = 0.0;

            var currentDate = DateTime.Now.Date;
            var sevenDaysAgo = currentDate.AddDays(-7);
            var ticketCreatedDates = totalTickets
                .Select(t => t.CreatedDate.Date)
                .Where(d => d >= currentDate.AddDays(-7))
                .ToList();
            var ticketResolvedDates = totalTickets
                .Select(t => t.ResolvedDate?.Date)
                .Where(d => d.HasValue && d.Value >= sevenDaysAgo)
                .Select(d => d.Value)
                .ToList();

            if (completedTicketsCount > 0)
            {
                var resolutionTime = new List<double>();
                foreach (var ticket in completedTickets)
                {
                    resolutionTime.Add((ticket.ResolvedDate.Value - ticket.TicketAssignment.AssignedDate).TotalMinutes);
                }
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
