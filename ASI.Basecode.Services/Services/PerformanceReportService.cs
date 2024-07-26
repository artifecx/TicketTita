using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.Services
{
    /// <summary>
    /// Service class for handling operations related to performance reports.
    /// </summary>
    public class PerformanceReportService : IPerformanceReportService
    {
        private readonly IPerformanceReportRepository _performanceReportRepository;
        private readonly ITeamRepository _teamRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceReportService"/> class.
        /// </summary>
        /// <param name="performanceReportRepository">The performance report repository.</param>
        /// <param name="teamRepository">The team repository.</param>
        public PerformanceReportService(IPerformanceReportRepository performanceReportRepository, ITeamRepository teamRepository)
        {
            _performanceReportRepository = performanceReportRepository;
            _teamRepository = teamRepository;
        }

        /// <summary>
        /// Generates the performance report for an agent asynchronously.
        /// </summary>
        /// <param name="agentId">The agent identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the generated performance report.</returns>
        public async Task<PerformanceReport> GenerateAgentPerformanceReportAsync(string agentId)
        {
            var agent = await _teamRepository.FindAgentByIdAsync(agentId);
            if (agent != null)
            {
                var performanceReport = agent.PerformanceReport;
                if (performanceReport == null)
                {
                    performanceReport = new PerformanceReport
                    {
                        ReportId = Guid.NewGuid().ToString(),
                        ResolvedTickets = 0,
                        AverageResolutionTime = 0.0,
                        AssignedDate = DateTime.UtcNow,
                        UserId = agentId
                    };
                    await _performanceReportRepository.AddPerformanceReportAsync(performanceReport);
                }

                var completedTickets = await _teamRepository.GetCompletedTicketsAssignedToAgentAsync(agentId);
                if (!completedTickets.Any()) return performanceReport;

                performanceReport.ResolvedTickets = completedTickets.Count;
                var resolutionTime = completedTickets.Select(ticket => (ticket.ResolvedDate.Value - ticket.TicketAssignment.AssignedDate).TotalMinutes).ToList();

                performanceReport.AverageResolutionTime = resolutionTime.Average();
                await _performanceReportRepository.UpdatePerformanceReportAsync(performanceReport);
                return performanceReport;
            }
            return null;
        }

        /// <summary>
        /// Gets the performance report for a user asynchronously.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the performance report view model.</returns>
        public async Task<PerformanceReportViewModel> GetPerformanceReport(string userId)
        {
            var user = await _teamRepository.FindAgentByIdAsync(userId);
            if (user != null)
            {
                var performanceReport = user.PerformanceReport;
                var tickets = await _teamRepository.GetCompletedTicketsAssignedToAgentAsync(userId);
                if (tickets.Any() && performanceReport != null)
                {
                    return new PerformanceReportViewModel
                    {
                        ReportId = performanceReport.ReportId,
                        ResolvedTickets = performanceReport.ResolvedTickets,
                        AverageResolutionTime = performanceReport.AverageResolutionTime,
                        AssignedDate = performanceReport.AssignedDate,
                        Name = user.Name,
                        AverageRating = tickets.Where(t => t.Feedback != null).Select(t => t.Feedback.FeedbackRating).Average(),
                        Feedbacks = tickets.Where(t => t.Feedback != null).Select(t => t.Feedback).ToList()
                    };
                }
            }
            return null;
        }
    }
}
