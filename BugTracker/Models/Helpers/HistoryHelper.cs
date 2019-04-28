using BugTracker.Models.Domain;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BugTracker.Models.Helpers
{
    public class HistoryHelper
    {
        private ApplicationDbContext DbContext;

        public HistoryHelper(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public void CreateHistory(string oldValue, string newValue, int ticketId, string property)
        {
            var ticketHistory = new TicketHistory();

            ticketHistory.OldValue = oldValue;
            ticketHistory.NewValue = newValue;
            ticketHistory.TicketId = ticketId;
            ticketHistory.Property = property;
            ticketHistory.Changed = DateTime.Now;
            ticketHistory.UserId = HttpContext.Current.User.Identity.GetUserId();
            DbContext.TicketHistories.Add(ticketHistory);
            DbContext.SaveChanges();
        }
    }
}