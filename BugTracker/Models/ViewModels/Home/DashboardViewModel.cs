using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels.Home
{
    public class DashboardViewModel
    {
        public int? NumberProjects {get; set;}
        public int? NumberTickets { get; set; }
        public int? NumberTicketsAssigned { get; set; }
        public int? NumberTicketsOwned { get; set; }
        public int? NumberOpenTickets { get; set; }
        public int? NumberResolvedTickets { get; set; }
        public int? NumberRejectedTickets { get; set; }
    }
}