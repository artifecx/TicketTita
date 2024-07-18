using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.ServiceModels
{
    public class PerformanceReportViewModel
    {
        public string ReportId { get; set; }
        public int ResolvedTickets { get; set; }
        public double AverageResolutionTime { get; set; }
        public DateTime AssignedDate { get; set; }
        public string Name { get; set; }
        public double AverageRating { get; set; }
        public List<Feedback> Feedbacks { get; set; }
    }
}
