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

        //For call inside action
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

        //For call from SaveChanges();
        public void CreatePropertyHistory(int ticketId, string property, string oldValue, string newValue)
        {

            int intOriginalValue = 0;
            int intCurrentValue = 0;
            var oldValueIsNumber = int.TryParse(oldValue, out intOriginalValue);
            var newValueIsNumber = int.TryParse(newValue, out intCurrentValue);

            var ticketHistory = new TicketHistory()
            {
                TicketId = ticketId,
                Changed = DateTime.Now,
                UserId = HttpContext.Current.User.Identity.GetUserId()
            };

            if (property == "ProjectId")
            {
                ticketHistory.Property = "Project";
                ticketHistory.OldValue = DbContext.Projects.FirstOrDefault(p => p.Id == intOriginalValue).Name;
                ticketHistory.NewValue = DbContext.Projects.FirstOrDefault(p => p.Id == intCurrentValue).Name;
            }
            else if (property == "TicketTypeId")
            {
                ticketHistory.Property = "Type";
                ticketHistory.OldValue = DbContext.TicketTypes.FirstOrDefault(p => p.Id == intOriginalValue).Name;
                ticketHistory.NewValue = DbContext.TicketTypes.FirstOrDefault(p => p.Id == intCurrentValue).Name;
            }
            else if (property == "TicketStatusId")
            {
                ticketHistory.Property = "Status";
                ticketHistory.OldValue = DbContext.TicketStatuses.FirstOrDefault(p => p.Id == intOriginalValue).Name;
                ticketHistory.NewValue = DbContext.TicketStatuses.FirstOrDefault(p => p.Id == intCurrentValue).Name;
            }
            else if (property == "TicketPriorityId")
            {
                ticketHistory.Property = "Priority";
                ticketHistory.OldValue = DbContext.TicketPriorities.FirstOrDefault(p => p.Id == intOriginalValue).Name;
                ticketHistory.NewValue = DbContext.TicketPriorities.FirstOrDefault(p => p.Id == intCurrentValue).Name;
            }
            else
            {
                ticketHistory.OldValue = oldValue;
                ticketHistory.NewValue = newValue;
                ticketHistory.Property = property;
            }

            DbContext.TicketHistories.Add(ticketHistory);
            DbContext.SaveChanges();
        }
    }
}