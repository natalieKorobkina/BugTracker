using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.Filters;
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

        public CommentController()
        {
            DbContext = new ApplicationDbContext();
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
        public ActionResult CreateComment(int? id, CreateCommentViewModel formData)
        {
            TicketComment comment  = new TicketComment();
            comment.Comment = formData.Comment;
            comment.DateCreated = DateTime.Now;
            comment.TicketId = id.Value;
            comment.UserId = User.Identity.GetUserId();

            DbContext.TicketComments.Add(comment);
            DbContext.SaveChanges();

            return RedirectToAction("TicketDetails", "Ticket", new { id = comment.TicketId }); 
        }
    }
}