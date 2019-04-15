using BugTracker.Models;
using BugTracker.Models.Domain;
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

        [Authorize]
        [HttpGet]
        public ActionResult CreateComment(int? id)
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult CreateComment(int? id, CreateCommentViewModel formData)
        {
            var currentTicket = DbContext.Tickets.Where(p => p.Id == id).FirstOrDefault();
            var userId = User.Identity.GetUserId();

            if (!User.IsInRole("Admin") && !User.IsInRole("ProjectManager"))
            {
                if (User.IsInRole("Developer") && (currentTicket.AssignedToUserId != userId))
                {
                    return RedirectToAction(nameof(TicketController.AllTickets));
                }

                if (User.IsInRole("Submitter") && (currentTicket.OwnerUserId != userId))
                {
                    return RedirectToAction(nameof(TicketController.AllTickets));
                }
            }

            TicketComment comment  = new TicketComment();
            comment.Comment = formData.Comment;
            comment.DateCreated = DateTime.Now;
            comment.TicketId = id.Value;
            comment.UserId = User.Identity.GetUserId();
            DbContext.TicketComments.Add(comment);
            DbContext.SaveChanges();

            return RedirectToAction("AllTickets", "Ticket"); 
        }
    }
}