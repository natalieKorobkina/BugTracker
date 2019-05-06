using BugTracker.Models.Helpers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BugTracker.Models.Filters
{
    public class CanActFilter : ActionFilterAttribute
    {
        public ApplicationDbContext DbContext = new ApplicationDbContext();
        private BugTrackerHelper bugTrackerHelper;

        public CanActFilter()
        {
            bugTrackerHelper = new BugTrackerHelper(DbContext);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var actionParameter = filterContext.ActionParameters.SingleOrDefault(p => p.Key == "id").Value.ToString();

            if (string.IsNullOrEmpty(actionParameter))
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
                    { "controller", "Ticket" }, { "action", "AllTickets" } });
            }

            int ticketId = Convert.ToInt32(actionParameter);
            var userId = HttpContext.Current.User.Identity.GetUserId();
            var isSubmitter = HttpContext.Current.User.IsInRole("Submitter");
            var isDeveloper = HttpContext.Current.User.IsInRole("Developer");
            var isAdminManager = HttpContext.Current.User.IsInRole("Admin")
                || HttpContext.Current.User.IsInRole("ProjectManager");
            var ticket = bugTrackerHelper.GetCurrentTicketById(ticketId);
            bool canCreate = false;

            if (ticket == null) {

                filterContext.Result = new ViewResult() { ViewName = "ItemError" };
            }
             else
            {
                if (isDeveloper && isSubmitter)
                {
                    canCreate = (userId == ticket.AssignedToUserId || userId == ticket.OwnerUserId);
                }
                else if (isDeveloper)
                {
                    canCreate = userId == ticket.AssignedToUserId;
                }
                else if (isSubmitter)
                {
                    canCreate = userId == ticket.OwnerUserId;
                }
                if (isAdminManager)
                {
                    canCreate = true;
                }
            }   

            filterContext.Controller.ViewData.Add("CanCreate", canCreate);
        }
    }
}