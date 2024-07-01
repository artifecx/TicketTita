using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class PerformanceReport
    {
        public PerformanceReport()
        {
            TeamMembers = new HashSet<TeamMember>();
        }

        public string ReportId { get; set; }
        public int ResolvedTickets { get; set; }
        public double AverageResolutionTime { get; set; }
        public DateTime AssignedDate { get; set; }

        public virtual ICollection<TeamMember> TeamMembers { get; set; }
    }
}
