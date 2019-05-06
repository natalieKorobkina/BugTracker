using BugTracker.Controllers;
using BugTracker.Models.Domain;
using BugTracker.Models.Helpers;
using BugTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BugTracker.Models.Filters
{
    public class HasRightsCheckFilter: ActionFilterAttribute
    {
        public ApplicationDbContext DbContext = new ApplicationDbContext();
        public virtual IDictionary<string, object> ActionArguments { get; }
        private BugTrackerHelper bugTrackerHelper;

        public HasRightsCheckFilter()
        {
            bugTrackerHelper = new BugTrackerHelper(DbContext);
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var actionParamentr = filterContext.ActionParameters.SingleOrDefault(p => p.Key == "id").Value.ToString();

            if (string.IsNullOrEmpty(actionParamentr))
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
                    { "controller", "Ticket" }, { "action", "AllTickets" } });
            }

            int ticketId = Convert.ToInt32(actionParamentr);
            var userId = HttpContext.Current.User.Identity.GetUserId();
            var isSubmitter = HttpContext.Current.User.IsInRole("Submitter");
            var isDeveloper = HttpContext.Current.User.IsInRole("Developer");
            var isAdminManager = HttpContext.Current.User.IsInRole("Admin") 
                || HttpContext.Current.User.IsInRole("ProjectManager");
            var ticket = bugTrackerHelper.GetCurrentTicketById(ticketId);

            if (ticket == null)
            {
                filterContext.Result = new ViewResult()
                {
                    ViewName = "ItemError"
                };
            }
            else
            {
                if (!isAdminManager)
                {
                    if ((ticket.AssignedToUserId != null && isDeveloper) || isDeveloper && (ticket.AssignedToUserId != userId))
                        filterContext.Result = new ViewResult()
                        {
                            ViewName = "AutorizationError"
                        };

                    if (isSubmitter && (ticket.OwnerUserId != userId))
                        filterContext.Result = new ViewResult()
                        {
                            ViewName = "AutorizationError"
                        };
                }
            }
        }
    }
}