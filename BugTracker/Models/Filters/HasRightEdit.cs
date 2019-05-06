using BugTracker.Models.Domain;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BugTracker.Models.Filters
{
    public class HasRightEdit : ActionFilterAttribute
    {
        public ApplicationDbContext DbContext = new ApplicationDbContext();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var actionAttachmentParameter = filterContext.ActionParameters
                .SingleOrDefault(p => p.Key == "attachmentId").Value;
            var actionCommentParameter = filterContext.ActionParameters
                .SingleOrDefault(p => p.Key == "commentId").Value;

            if ((actionAttachmentParameter == null) && (actionCommentParameter == null))
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
                    { "controller", "Ticket" }, { "action", "TicketDetails" }, {"id", "actionSecondParameter" }  });
            }

            var userId = HttpContext.Current.User.Identity.GetUserId();
            var isAdminManager = HttpContext.Current.User.IsInRole("Admin") || HttpContext.Current.User.IsInRole("ProjectManager");
            int itemId = 0;
            TicketAttachment attachment = null;
            TicketComment comment = null;

            if (actionAttachmentParameter != null)
            {
                itemId = Convert.ToInt32(actionAttachmentParameter.ToString());
                attachment = DbContext.TicketAttachments.FirstOrDefault(a => a.Id == itemId && a.Archived == false);
            }

            if (actionCommentParameter != null)
            {
                itemId = Convert.ToInt32(actionCommentParameter.ToString());
                comment = DbContext.TicketComments.FirstOrDefault(c => c.Id == itemId && c.Archived == false);
            }

            if (attachment == null && comment ==null)
            {
                filterContext.Result = new ViewResult() { ViewName = "ItemError" };
            }
            else
            {
                if (!isAdminManager)
                {
                    if ((attachment != null) && (attachment.UserId != userId))
                        filterContext.Result = new ViewResult() { ViewName = "AutorizationError" };

                    if ((comment != null) && (comment.UserId != userId))
                        filterContext.Result = new ViewResult() { ViewName = "AutorizationError" };
                }
            }  
        }   
     }
}