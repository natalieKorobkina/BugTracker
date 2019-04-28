using AutoMapper;
using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.Filters;
using BugTracker.Models.Helpers;
using BugTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class CommentController : Controller
    {
        private ApplicationDbContext DbContext;
        private BugTrackerHelper bugTrackerHelper;
        private NotificationHelper notificationHelper;

        public CommentController()
        {
            DbContext = new ApplicationDbContext();
            bugTrackerHelper = new BugTrackerHelper(DbContext);
            notificationHelper = new NotificationHelper(DbContext);
        }

        [HttpGet]
        [Authorize]
        [HasRightsCheckFilter()]
        public ActionResult CreateComment(int? id)
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [HasRightsCheckFilter()]
        public ActionResult CreateComment(int? id, CreateEditCommentViewModel formData)
        {
            return SaveComment(id, null, formData);
        }

        [HttpGet]
        [Authorize]
        [HasRightEdit]
        public ActionResult EditComment(int? id, int? commentId)
        {
            var model = new CreateEditCommentViewModel();
            var comment = bugTrackerHelper.GetCommentById(commentId.Value);

            model = Mapper.Map<CreateEditCommentViewModel>(comment);

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [HasRightEdit]
        public ActionResult EditComment(int? id, int? commentId, CreateEditCommentViewModel formData)
        {
            return SaveComment(id, commentId, formData);
        }

        private ActionResult SaveComment(int? id, int? commentId, CreateEditCommentViewModel formData)
        {
            if (!ModelState.IsValid || !id.HasValue)
                return RedirectToAction("AllTickets", "Ticket");

            TicketComment comment = new TicketComment();
            var ticket = bugTrackerHelper.GetCurrentTicketById(id.Value);
            var message = notificationHelper.CreateCommentNotification(ticket.Title);

            if (commentId == null)
            {
                comment.DateCreated = DateTime.Now;
                comment.TicketId = id.Value;
                comment.UserId = User.Identity.GetUserId();
                DbContext.TicketComments.Add(comment);
                notificationHelper.SendNotification(ticket, message, false);
            }
            else
            {
                comment = bugTrackerHelper.GetCommentById(commentId.Value);
            }

            comment.Comment = formData.Comment;
            DbContext.SaveChanges();

            return RedirectToAction("TicketDetails", "Ticket", new { id = comment.TicketId });
        }

        [HttpPost]
        [Authorize]
        [HasRightEdit]
        public ActionResult Delete(int? commentId)
        {
            if (commentId.HasValue)
            {
                var comment = bugTrackerHelper.GetCommentById(commentId);

                if (comment != null)
                {
                    DbContext.TicketComments.Remove(comment);
                    DbContext.SaveChanges();
                }

                return RedirectToAction("TicketDetails", "Ticket", new { id = comment.TicketId });
            }

            return RedirectToAction("Alltickets", "Ticket");
        }
    }
}