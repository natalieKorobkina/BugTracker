using AutoMapper;
using AutoMapper.QueryableExtensions;
using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.Filters;
using BugTracker.Models.Helpers;
using BugTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class TicketController : Controller
    {
        private ApplicationDbContext DbContext;
        private BugTrackerHelper bugTrackerHelper;
        private UserManager<ApplicationUser> userManager;
        private string userId;

        public TicketController()
        {
            DbContext = new ApplicationDbContext();
            bugTrackerHelper = new BugTrackerHelper(DbContext);
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
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
                model = DbContext.Tickets.ProjectTo<AllTicketsViewModel>().ToList();
                model.ForEach(t => t.EditAvailable = true);

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

        [HttpGet]
        [Authorize(Roles = "Submitter")]
        public ActionResult Create()
        {
            var model = new CreateTicketViewModel();

            userId = User.Identity.GetUserId();
            model.Projects = bugTrackerHelper.GetDropDownListProjectsCreate(userId);
            model.Types = bugTrackerHelper.GetDropDownListTypes();
            model.Priorities = bugTrackerHelper.GetDropDownListPriorities();

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Submitter")]
        public ActionResult Create(CreateTicketViewModel formData)
        {
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

            model.Projects = bugTrackerHelper.GetDropDownListProjects();
            model.Types = bugTrackerHelper.GetDropDownListTypes();
            model.Priorities = bugTrackerHelper.GetDropDownListPriorities();
            
            if (User.IsInRole("Admin") || User.IsInRole("ProjectManager"))
            {
                model.Statuses = bugTrackerHelper.GetDropDownListProjects();
                model.Projects = bugTrackerHelper.GetDropDownListProjects();
            }
            else
            {
                model.Projects = bugTrackerHelper.GetDropDownListProjectsCreate(userId);
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public ActionResult Edit(int? id, EditTicketViewModel formData)
        {
            var ticket = bugTrackerHelper.GetCurrentTicketById(id.Value);

            if (id == null || ticket == null)
            {
                return RedirectToAction(nameof(HomeController.Index));
            }

            ticket.Title = formData.Title;
            ticket.Description = formData.Description;
            ticket.ProjectId = formData.ProjectId;
            ticket.TicketTypeId = formData.TicketTypeId;
            ticket.TicketPriorityId = formData.TicketPriorityId;
            ticket.DateUpdated = DateTime.Now;

            if (User.IsInRole("Admin") || User.IsInRole("ProjectManager"))
            {
                ticket.TicketStatusId = formData.TicketStatusId;
            }
            
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
                return RedirectToAction(nameof(TicketController.AllTickets));

                ticket.AssignedToUserId = formData.DeveloperId;
                DbContext.SaveChanges();

            return RedirectToAction(nameof(TicketController.AllTickets));
        }

        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager, Developer, Submitter")]
        [CanActFilter()]
        public ActionResult TicketDetails(int? id)
        {
            var ticket = DbContext.Tickets.Where(t => t.Id == id).FirstOrDefault();
            var model = new TicketDetailsViewModel();

            model = Mapper.Map<TicketDetailsViewModel>(ticket);
            var temp = ViewData["CanCreate"];
            model.CanCreate = (bool)ViewData["CanCreate"];

            return View(model);
        }
    }
}