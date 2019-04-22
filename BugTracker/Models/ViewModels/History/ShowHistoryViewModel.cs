using BugTracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels.History
{
    public class ShowHistoryViewModel
    {
        public string Property { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime? Changed { get; set; }

        public Ticket Ticket { get; set; }
        public ApplicationUser User { get; set; }
    }
}