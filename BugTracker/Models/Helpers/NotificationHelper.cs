using BugTracker.Models.Domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Helpers
{
    public class NotificationHelper
    {
        private ApplicationDbContext DbContext;
        private BugTrackerHelper bugTrackerHelper;
        private EmailService emailService;
        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        public NotificationHelper(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
            bugTrackerHelper = new BugTrackerHelper(DbContext);
            emailService = new EmailService();
        }

        public class Email
        {
            public string Title { get; set; }
            public string Body { get; set; }
        }

        public bool CheckIfDeveloperDidChanges(Ticket ticket)
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();

            return ticket.AssignedToUserId != userId;
        }

        public void SendNotification(Ticket ticket, Email message, bool assignment)
        {
            if (ticket.AssignedToUserId != null && (assignment == true || CheckIfDeveloperDidChanges(ticket)))
            {
                //Send email to the developer
                var developerEmail = bugTrackerHelper.GetUserById(ticket.AssignedToUserId).Email;

                emailService.Send(developerEmail, message.Body, message.Title);

                //Send emails subscribed admins/PMs
                if (!assignment)
                {
                    var allSubscribers = DbContext.TicketNotifications
                        .Where(n => n.TicketId == ticket.Id).Select(n => n.User.Email).ToList();
                    if (allSubscribers.Any())
                    {
                        var adressesString = string.Join(", ", allSubscribers); 

                        emailService.Send(adressesString, message.Body, message.Title);
                    }
                }
            }
        }

        public Email CreateAssignmentNotification(string ticketTitle)
        {
            var email = new Email();

            email.Title = "Ticket assignment";
            email.Body = "You were assigned to the ticket: " + "'" + ticketTitle + "'";

            return email;
        }

        public Email CreateModificationNotification(string ticketTitle)
        {
            var email = new Email();

            email.Title = "Ticket modification";
            email.Body = "The ticket: " + "'" + ticketTitle + "'" + ", to which you are assigned/subscribed has been modified";

            return email;
        }

        public Email CreateAttachmentNotification(string ticketTitle)
        {
            var email = new Email();

            email.Title = "Attachment modification";
            email.Body = "There was attachment's addition to the ticket: " + "'" + ticketTitle + "'" + ", to which you are assigned/subscribed";

            return email;
        }

        public Email CreateCommentNotification(string ticketTitle)
        {
            var email = new Email();

            email.Title = "Comment modification";
            email.Body = "There was comment's addition to the ticket: " + "'" + ticketTitle + "'" + ", to which you are assigned/subscribed";

            return email;
        }
    }
}