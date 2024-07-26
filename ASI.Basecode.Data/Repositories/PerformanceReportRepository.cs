using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Repositories
{
    /// <summary>
    /// Repository class for handling operations related to the PerformanceReport entity.
    /// </summary>
    public class PerformanceReportRepository : BaseRepository, IPerformanceReportRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceReportRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        public PerformanceReportRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        /// <summary>
        /// Adds the performance report asynchronously.
        /// </summary>
        /// <param name="performanceReport">The performance report to add.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task AddPerformanceReportAsync(PerformanceReport performanceReport)
        {
            await this.GetDbSet<PerformanceReport>().AddAsync(performanceReport);
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Updates the performance report asynchronously.
        /// </summary>
        /// <param name="performanceReport">The performance report to update.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task UpdatePerformanceReportAsync(PerformanceReport performanceReport)
        {
            this.GetDbSet<PerformanceReport>().Update(performanceReport);
            await UnitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves the performance report by agent identifier asynchronously.
        /// </summary>
        /// <param name="agentId">The agent identifier.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation. The task result contains the performance report associated with the specified agent identifier.</returns>
        public async Task<PerformanceReport> GetPerformanceReportByAgentIdAsync(string agentId)
            => await this.GetDbSet<PerformanceReport>().Where(r => r.UserId == agentId).FirstOrDefaultAsync();
    }
}
