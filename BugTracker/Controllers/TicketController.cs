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
                    ticket.OffNotification = DbContext.TicketNotifications
                        .FirstOrDefault(n => n.TicketId == ticket.Id && n.UserId == userId) != null;
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
            var notification = DbContext.TicketNotifications.FirstOrDefault(n => n.TicketId == ticket.Id && n.UserId == userId);

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
            bool wasModifyed = false;

            if (!ModelState.IsValid || id == null || ticket == null)
            {
                return RedirectToAction(nameof(TicketController.AllTickets));
            }

            if (ticket.Title != formData.Title)
            {
                historyHelper.CreateHistory(ticket.Title, formData.Title, ticket.Id, "Title");
                wasModifyed = true;
            }

            if (ticket.Description != formData.Description)
            {
                historyHelper.CreateHistory(ticket.Description, formData.Description, ticket.Id, "Description");
                wasModifyed = true;
            }

            if (ticket.ProjectId != formData.ProjectId)
            {
                var newProjectName = bugTrackerHelper.GetProjectNameById(formData.ProjectId);
                historyHelper.CreateHistory(ticket.Project.Name, newProjectName, ticket.Id, "Project");
                wasModifyed = true;
            }

            if (ticket.TicketPriorityId != formData.TicketPriorityId)
            {
                var newPriority = DbContext.TicketPriorities.FirstOrDefault(p => p.Id == formData.TicketPriorityId);
                historyHelper.CreateHistory(ticket.TicketPriority.Name, newPriority.Name, ticket.Id, "Priority");
                wasModifyed = true;
            }

            if (ticket.TicketTypeId != formData.TicketTypeId)
            {
                var newType = DbContext.TicketTypes.FirstOrDefault(p => p.Id == formData.ProjectId);
                historyHelper.CreateHistory(ticket.TicketType.Name, newType.Name, ticket.Id, "Type");
                wasModifyed = true;
            }

            if (formData.TicketStatusId != 0 && ticket.TicketStatusId != formData.TicketStatusId)
            {
                var newStatus = DbContext.TicketStatuses.FirstOrDefault(p => p.Id == formData.TicketStatusId);
                historyHelper.CreateHistory(ticket.TicketStatus.Name, newStatus.Name, ticket.Id, "Status");
                wasModifyed = true;
            }

            //ticket = Mapper.Map<Ticket>(formData);

            ticket.Title = formData.Title;
            ticket.Description = formData.Description;
            ticket.ProjectId = formData.ProjectId;
            ticket.TicketTypeId = formData.TicketTypeId;
            ticket.TicketPriorityId = formData.TicketPriorityId;
            ticket.DateUpdated = DateTime.Now;

            if (User.IsInRole("Admin") || User.IsInRole("ProjectManager"))
                ticket.TicketStatusId = formData.TicketStatusId;

            DbContext.SaveChanges();

            if(wasModifyed)
            {
                var message = notificationHelper.CreateModificationNotification(ticket.Title);

                notificationHelper.SendNotification(ticket, message, false);
            }
            
            return RedirectToAction(nameof(TicketController.AllTickets));
        }

        public void ReflectiveEquals(object first, object second)
        {
            Type firstType = first.GetType();
            if (second.GetType() != firstType)
            {
               // return false; // Or throw an exception
            }

            foreach (PropertyInfo propertyInfo in firstType.GetProperties())
            {
                if (propertyInfo.CanRead)
                {
                    object firstValue = propertyInfo.GetValue(first, null);
                    object secondValue = propertyInfo.GetValue(second, null);
                    if (!object.Equals(firstValue, secondValue))
                    {
                        //return false;
                    }
                }
            }
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
            var message = notificationHelper.CreateAssignmentNotification(ticket.Title);

            if (id == null || ticket == null)
                return RedirectToAction("AllTickets", "Ticket");

            if (ticket.AssignedToUserId != formData.DeveloperId)
            {
                var newDeveloper = DbContext.Users.FirstOrDefault(p => p.Id == formData.DeveloperId);
                if (ticket.AssignedToUserId != null)
                {
                    historyHelper.CreateHistory(ticket.AssignedToUser.DisplayName,
                    newDeveloper.DisplayName, ticket.Id, "Assigned Developer");

                    var messageModification = notificationHelper.CreateModificationNotification(ticket.Title);
                    notificationHelper.SendNotification(ticket, messageModification, false);
                }
            }

            ticket.AssignedToUserId = formData.DeveloperId;
            DbContext.SaveChanges();
            notificationHelper.SendNotification(ticket, message, true);
            
            return RedirectToAction("AllTickets", "Ticket");
        }

        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager, Developer, Submitter")]
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