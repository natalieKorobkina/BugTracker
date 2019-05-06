using AutoMapper;
using AutoMapper.QueryableExtensions;
using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.Filters;
using BugTracker.Models.Helpers;
using BugTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class TicketController : Controller
    {
        private ApplicationDbContext DbContext;
        private BugTrackerHelper bugTrackerHelper;
        private NotificationHelper notificationHelper;
        private string userId;
        private HistoryHelper historyHelper;
        private ApplicationUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        public TicketController()
        {
            DbContext = new ApplicationDbContext();
            bugTrackerHelper = new BugTrackerHelper(DbContext);
            notificationHelper = new NotificationHelper(DbContext);
            historyHelper = new HistoryHelper(DbContext);
        }

        [Authorize(Roles = "Admin, ProjectManager, Developer, Submitter")]
        public ActionResult AllTickets()
        {
            userId = User.Identity.GetUserId();

            var isSubmitter = User.IsInRole("Submitter");
            var isDeveloper = User.IsInRole("Developer");
            var isAdminManager = User.IsInRole("Admin") || User.IsInRole("ProjectManager");
            var model = new List<AllTicketsViewModel>();

            if (isAdminManager)
            {
                model = bugTrackerHelper.ActiveTickets().ProjectTo<AllTicketsViewModel>().ToList();
                model.ForEach(t => t.EditAvailable = true);

                foreach (var ticket in model)
                {
                    ticket.OffNotification = notificationHelper.GetNotificationByTicketUserIds(ticket.Id, userId) != null;
                }

                return View(model);
            }

            if (isDeveloper && isSubmitter)
            {
                model = bugTrackerHelper.GetTicketsForDevSubmitters(userId).ProjectTo<AllTicketsViewModel>().ToList();
                model.ForEach(t => t.EditAvailable = bugTrackerHelper.IsAssigned(t, userId)
                || bugTrackerHelper.IsOwned(t, userId));

                return View(model);
            }

            if (isDeveloper)
            {
                model = bugTrackerHelper.GetTicketsForDeveloper(userId).ProjectTo<AllTicketsViewModel>().ToList();
                model.ForEach(t => t.EditAvailable = bugTrackerHelper.IsAssigned(t, userId));

                return View(model);
            }

            if (isSubmitter)
            {
                model = bugTrackerHelper.GetTicketsForSubmitters(userId).ProjectTo<AllTicketsViewModel>().ToList();
                model.ForEach(t => t.EditAvailable = bugTrackerHelper.IsOwned(t, userId));

                return View(model);
            }

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult Alltickets(int? id, int? sendNotification)
        {
            var userId = User.Identity.GetUserId();
            var ticket = bugTrackerHelper.GetCurrentTicketById(id.Value);
            var notification = notificationHelper.GetNotificationByTicketUserIds(ticket.Id, userId);

            if (notification == null && sendNotification != null)
            {
                var ticketNotification = new TicketNotification();

                ticketNotification.TicketId = ticket.Id;
                ticketNotification.UserId = userId;
                DbContext.TicketNotifications.Add(ticketNotification);
            }

            if (notification != null && sendNotification == null)
            {
                DbContext.TicketNotifications.Remove(notification);
            }

            DbContext.SaveChanges();

            return RedirectToAction("AllTickets", "Ticket");
        }

        [HttpGet]
        [Authorize(Roles = "Submitter")]
        public ActionResult Create()
        {
            var model = new CreateTicketViewModel();

            userId = User.Identity.GetUserId();
            model.Projects = bugTrackerHelper.GetDropDownListUsersProjects(userId);
            model.Types = bugTrackerHelper.GetDropDownListTypes();
            model.Priorities = bugTrackerHelper.GetDropDownListPriorities();

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Submitter")]
        public ActionResult Create(CreateTicketViewModel formData)
        {
            if (!ModelState.IsValid)
                return View();

            userId = User.Identity.GetUserId();
            Ticket ticket = new Ticket();

            ticket = Mapper.Map<Ticket>(formData);
            ticket.DateCreated = DateTime.Now;
            ticket.TicketStatusId = bugTrackerHelper.GetStatusOpen();
            ticket.OwnerUserId = userId;

            DbContext.Tickets.Add(ticket);
            DbContext.SaveChanges();

            return RedirectToAction(nameof(TicketController.AllTickets));
        }

        [Authorize]
        [HasRightsCheckFilter()]
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            var model = new EditTicketViewModel();

            userId = User.Identity.GetUserId();

            var currentTicket = bugTrackerHelper.GetCurrentTicketById(id.Value);

            model = Mapper.Map<EditTicketViewModel>(currentTicket);

            model.Types = bugTrackerHelper.GetDropDownListTypes();
            model.Priorities = bugTrackerHelper.GetDropDownListPriorities();

            if (User.IsInRole("Admin") || User.IsInRole("ProjectManager"))
            {
                model.Statuses = bugTrackerHelper.GetDropDownListStatuses();
                model.Projects = bugTrackerHelper.GetDropDownListProjects();
            }
            else
            {
                model.Projects = bugTrackerHelper.GetDropDownListUsersProjectsEdit(userId, currentTicket.ProjectId);
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [HasRightsCheckFilter()]
        public ActionResult Edit(int? id, EditTicketViewModel formData)
        {
            var ticket = bugTrackerHelper.GetCurrentTicketById(id.Value);

            ticket.Title = formData.Title;
            ticket.Description = formData.Description;
            ticket.ProjectId = formData.ProjectId;
            ticket.TicketTypeId = formData.TicketTypeId;
            ticket.TicketPriorityId = formData.TicketPriorityId;
            ticket.DateUpdated = DateTime.Now;

            if (User.IsInRole("Admin") || User.IsInRole("ProjectManager"))
                ticket.TicketStatusId = formData.TicketStatusId;

            DbContext.SaveChanges();

            return RedirectToAction(nameof(TicketController.AllTickets));
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpGet]
        public ActionResult TicketsAssignment(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(TicketController.AllTickets));

            var model = new TicketsAssignmentViewModel();
            var currentTicket = bugTrackerHelper.GetCurrentTicketById(id.Value);

            model.Developers = bugTrackerHelper.GetAllDevelopers();
            model.DeveloperId = currentTicket.AssignedToUserId;

            return View(model);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        public ActionResult TicketsAssignment(int? id, TicketsAssignmentViewModel formData)
        {
            var ticket = bugTrackerHelper.GetCurrentTicketById(id.Value);

            if (id == null || ticket == null)
                return RedirectToAction("AllTickets", "Ticket");

            if (ticket.AssignedToUserId != formData.DeveloperId)
            {
                var newDeveloper = DbContext.Users.FirstOrDefault(p => p.Id == formData.DeveloperId);
                var oldValue = (ticket.AssignedToUserId == null) ? "not assigned" : ticket.AssignedToUser.DisplayName;

                historyHelper.CreateHistory(oldValue, newDeveloper.DisplayName, ticket.Id, "Assigned Developer");

                ticket.AssignedToUserId = formData.DeveloperId;
                var message = notificationHelper.CreateAssignmentNotification(ticket.Title, newDeveloper.DisplayName);
                notificationHelper.SendNotification(ticket, message, true);
            }

            DbContext.SaveChanges();

            return RedirectToAction("AllTickets", "Ticket");
        }

        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager, Developer, Submitter")]
        [HasRightsCheckFilter]
        [CanActFilter()]
        public ActionResult TicketDetails(int? id)
        {
            var ticket = bugTrackerHelper.GetCurrentTicketById(id.Value);
            var model = new TicketDetailsViewModel();

            model = Mapper.Map<TicketDetailsViewModel>(ticket);
            model.CanCreate = (bool)ViewData["CanCreate"];
            model.TicketAttachments = bugTrackerHelper.GetListAttachments(ticket);
            model.TicketComments = bugTrackerHelper.GetListComments(ticket);
            model.TicketHistories = bugTrackerHelper.GetListHistories(ticket);

            return View(model);
        }
    }
}