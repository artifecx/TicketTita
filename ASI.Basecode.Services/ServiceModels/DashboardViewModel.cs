﻿using ASI.Basecode.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.ServiceModels
{
    public class DashboardViewModel
    {
        public int OpenTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int UnassignedTickets { get; set; }
        public int NewTickets { get; set; }
        public int CompletedTickets { get; set; }
        public double AverageResolutionTime { get; set; }
        public double AverageFeedbackRating { get; set; }
        public int FeedbacksCount { get; set; }
        public int TotalTicketsSoftware { get; set; }
        public int TotalTicketsHardware { get; set; }
        public int TotalTicketsNetwork { get; set; }
        public int TotalTicketsAccount { get; set; }
        public int TotalTicketsOther { get; set; }

        public List<DateTime> TicketCreatedDates { get; set; }
        public List<DateTime> TicketResolvedDates { get; set; }
        public List<int> FeedbackRatings { get; set; }
    }
}
