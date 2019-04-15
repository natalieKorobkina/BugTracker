using AutoMapper;
using AutoMapper.QueryableExtensions;
using BugTracker.Models;
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

                foreach (var ticket in model)
                {
                    ticket.EditAvailable = true;
                }

                return View(model);
            }

            if (isDeveloper && isSubmitter)
            {
                model = bugTrackerHelper.GetTicketsForDevSubmitters(userId).ProjectTo<AllTicketsViewModel>().ToList();

                foreach (var ticket in model)
                {
                    ticket.EditAvailable = ((ticket.AssignedToUser != null) && (userId == ticket.AssignedToUser.Id)) || (userId == ticket.OwnerUser.Id);
                }

                return View(model);
            }

            if (isDeveloper)
            {
                model = bugTrackerHelper.GetTicketsForDeveloper(userId).ProjectTo<AllTicketsViewModel>().ToList();

                foreach (var ticket in model)
                {
                    ticket.EditAvailable = (ticket.AssignedToUser != null) && (userId == ticket.AssignedToUser.Id);
                }

                return View(model);
            }
            if (isSubmitter)
            {
                model = bugTrackerHelper.GetTicketsForSubmitters(userId).ProjectTo<AllTicketsViewModel>().ToList();

                foreach (var ticket in model)
                {
                    ticket.EditAvailable = true && (userId == ticket.OwnerUser.Id);
                }

                return View(model);
            }
            return View();
        }

        //[Authorize(Roles = "Developer")]
        //public ActionResult DevelopersTickets()
        //{
        //    userId = User.Identity.GetUserId();
        //    var allIdOfUserProjects = DbContext.Users.Where(u => u.Id == userId)
        //        .SelectMany(u => u.Projects).Select(p => p.Id).ToList();
        //    var query = DbContext.Tickets.Where(p => allIdOfUserProjects.Contains(p.ProjectId)
        //        || p.AssignedToUserId == userId);

        //    var model = query.Select(p => new AllTicketsViewModel
        //    {
        //        Id = p.Id,
        //        Project = p.Project,
        //        Title = p.Title,
        //        DateCreated = p.DateCreated,
        //        DateUpdated = p.DateUpdated,
        //        TicketType = p.TicketType,
        //        TicketStatus = p.TicketStatus,
        //        TicketPriority = p.TicketPriority,
        //        OwnerUser = p.OwnerUser,
        //        AssignedToUser = p.AssignedToUser,
        //        //EditAvailable = true && (userId == p.AssignedToUserId)

        //    }).ToList();

        //    return View(model);
        //}

        //[Authorize(Roles = "Submitter")]
        //public ActionResult SubmitterTickets()
        //{
        //    userId = User.Identity.GetUserId();
        //    var allIdOfUserProjects = DbContext.Users.Where(u => u.Id == userId)
        //        .SelectMany(u => u.Projects).Select(p => p.Id).ToList();

        //    var query = DbContext.Tickets.Where(p => allIdOfUserProjects.Contains(p.ProjectId)
        //    || p.OwnerUserId == userId);

        //    var model = query.Select(p => new AllTicketsViewModel
        //    {
        //        Id = p.Id,
        //        Project = p.Project,
        //        Title = p.Title,
        //        DateCreated = p.DateCreated,
        //        DateUpdated = p.DateUpdated,
        //        TicketType = p.TicketType,
        //        TicketStatus = p.TicketStatus,
        //        TicketPriority = p.TicketPriority,
        //        OwnerUser = p.OwnerUser,
        //        AssignedToUser = p.AssignedToUser

        //    }).ToList();

        //    return View(model);
        //}

        [Authorize(Roles = "Submitter")]
        [HttpGet]
        public ActionResult Create()
        {
            var model = new CreateTicketViewModel();

            userId = User.Identity.GetUserId();
            model.Projects = bugTrackerHelper.GetUserProjectsById(userId).Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name

            }).ToList();

            model.Types = DbContext.TicketTypes.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();

            model.Priorities = DbContext.TicketPriorities.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Submitter")]
        public ActionResult Create(CreateTicketViewModel formData)
        {
            userId = User.Identity.GetUserId();

            Ticket ticket = new Ticket();
            ticket.Title = formData.Title;
            ticket.Discription = formData.Description;
            ticket.ProjectId = formData.ProjectId;
            ticket.TicketTypeId = formData.TypeId;
            ticket.TicketPriorityId = formData.PriorityId;
            ticket.DateCreated = DateTime.Now;
            ticket.DateUpdated = DateTime.Now;
            ticket.TicketStatusId = DbContext.TicketStatuses.Where(p => p.Name == "Open").Select(p => p.Id).FirstOrDefault();
            ticket.OwnerUserId = userId;

            DbContext.Tickets.Add(ticket);
            DbContext.SaveChanges();

            return RedirectToAction(nameof(TicketController.AllTickets));
        }

        [Authorize]
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            var model = new EditTicketViewModel();

            if (id == null)
                return RedirectToAction(nameof(TicketController.AllTickets));

            userId = User.Identity.GetUserId();
            var currentTicket = DbContext.Tickets.Where(p => p.Id == id).FirstOrDefault();

            if (!userManager.IsInRole(userId, "Admin") && !userManager.IsInRole(userId, "ProjectManager"))
            {
                if (userManager.IsInRole(userId, "Developer") && (currentTicket.AssignedToUserId != userId))
                {
                    return RedirectToAction(nameof(TicketController.AllTickets));
                }

                if (userManager.IsInRole(userId, "Submitter") && (currentTicket.OwnerUserId != userId))
                {
                    return RedirectToAction(nameof(TicketController.AllTickets));
                }
            }


            model.Title = currentTicket.Title;
            model.Description = currentTicket.Discription;

            model.Projects = DbContext.Projects.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name

            }).ToList();


            model.Types = DbContext.TicketTypes.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();

            model.Priorities = DbContext.TicketPriorities.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name
            }).ToList();

            model.ProjectId = currentTicket.ProjectId;
            model.TypeId = currentTicket.TicketTypeId;
            model.PriorityId = currentTicket.TicketPriorityId;
            model.DateCreated = currentTicket.DateCreated;

            userId = User.Identity.GetUserId();
            if (userManager.IsInRole(userId, "Admin") || userManager.IsInRole(userId, "ProjectManager"))
            {
                model.Statuses = DbContext.TicketStatuses.Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }).ToList();

                model.StatusId = currentTicket.TicketStatusId;
            }

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public ActionResult Edit(int? id, EditTicketViewModel formData)
        {
            userId = User.Identity.GetUserId();

            var ticket = DbContext.Tickets.Where(p => p.Id == id).FirstOrDefault();

            if (id == null || ticket == null)
            {
                return RedirectToAction(nameof(HomeController.Index));
            }

            ticket.Title = formData.Title;
            ticket.Discription = formData.Description;
            ticket.ProjectId = formData.ProjectId;
            ticket.TicketTypeId = formData.TypeId;
            ticket.TicketPriorityId = formData.PriorityId;
            ticket.DateUpdated = DateTime.Now;

            if (User.IsInRole("Admin") || User.IsInRole("ProjectManager"))
            {
                ticket.TicketStatusId = formData.StatusId;
            }

            DbContext.SaveChanges();
            return RedirectToAction(nameof(TicketController.AllTickets));
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpGet]
        public ActionResult TicketsAssignment(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(HomeController.Index));

            var model = new TicketsAssignmentViewModel();
            var currentTicket = DbContext.Tickets.Where(p => p.Id == id).FirstOrDefault();
            var developerRoleId = DbContext.Roles.Where(r => r.Name == "Developer").Select(p => p.Id).FirstOrDefault();

            model.Developers = DbContext.Users.Where(x => x.Roles.Select(y => y.RoleId).Contains(developerRoleId))
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.DisplayName

                }).ToList();

            model.DeveloperId = currentTicket.AssignedToUserId;

            return View(model);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        public ActionResult TicketsAssignment(int? id, TicketsAssignmentViewModel formData)
        {
            var ticket = DbContext.Tickets.Where(p => p.Id == id).FirstOrDefault();
            var developer = DbContext.Users.Where(p => p.Id == formData.DeveloperId).FirstOrDefault();

            if (id == null || ticket == null)
            {
                return RedirectToAction(nameof(HomeController.Index));
            }


            if (User.IsInRole("Admin") || User.IsInRole("ProjectManager"))
            {
                ticket.AssignedToUserId = formData.DeveloperId;

                DbContext.SaveChanges();

                return RedirectToAction(nameof(TicketController.AllTickets));
            }

            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager, Developer, Submitter")]
        public ActionResult TicketDetails(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(TicketController.AllTickets));
            }

            userId = User.Identity.GetUserId();

            var ticket = DbContext.Tickets.Where(t => t.Id == id).FirstOrDefault();
            var isSubmitter = User.IsInRole("Submitter");
            var isDeveloper = User.IsInRole("Developer");
            var isAdminManager = User.IsInRole("Developer") || User.IsInRole("ProjectManager");
            var model = new TicketDetailsViewModel();

            model = Mapper.Map<TicketDetailsViewModel>(ticket);

            if (isAdminManager)
            {
                model.CanCreate = true;

                return View(model);
            }
            if (isDeveloper && isSubmitter)
            {
                model.CanCreate = true && (userId == ticket.AssignedToUserId || userId == ticket.OwnerUserId);

                return View(model);
            }
            if (isDeveloper)
            {
                model.CanCreate = true && (userId == ticket.AssignedToUserId);

                return View(model);
            }
            model.CanCreate = true && (userId == ticket.OwnerUserId);

            return View(model);
        }

    }
}