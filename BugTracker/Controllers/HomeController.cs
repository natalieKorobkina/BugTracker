using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using BugTracker.Models.Helpers;
using BugTracker.Models.ViewModels.Home;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security;

namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext DbContext;
        private ApplicationUserManager UserManager;
        private BugTrackerHelper bugTrackerHelper;

        public HomeController()
        {
            DbContext = new ApplicationDbContext();
            UserManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            bugTrackerHelper = new BugTrackerHelper(DbContext);
        }

        [Authorize(Roles = "Admin, ProjectManager, Developer, Submitter")]
        public ActionResult Index()
        {
            var model = new DashboardViewModel();
            var userId = User.Identity.GetUserId();

            if (User.IsInRole("Admin") || User.IsInRole("ProjectManager"))
            {
                model.NumberProjects = bugTrackerHelper.GetNumberProject();
                model.NumberOpenTickets = bugTrackerHelper.GetListTicketsByStatus("Open");
                model.NumberRejectedTickets = bugTrackerHelper.GetListTicketsByStatus("Rejected");
                model.NumberResolvedTickets = bugTrackerHelper.GetListTicketsByStatus("Resolved");
                model.NumberTickets = bugTrackerHelper.ActiveTickets().Count();

                return View(model);
            }

            model.NumberProjects = bugTrackerHelper.GetUserProjectsById(userId).Count();

            if (User.IsInRole("Developer"))
            {
                model.NumberTicketsAssigned = bugTrackerHelper.NumberTicketsUserAssigned(userId);
                model.NumberTickets = bugTrackerHelper.GetTicketsForDeveloper(userId).Count();
            }
             
            if (User.IsInRole("Submitter"))
            {
                model.NumberTicketsOwned = bugTrackerHelper.NumberTicketsUserOwned(userId);
                model.NumberTickets = bugTrackerHelper.GetTicketsForSubmitters(userId).Count();
            }

            if (User.IsInRole("Submitter") && User.IsInRole("Developer"))
            {
                model.NumberTicketsAssigned = bugTrackerHelper.NumberTicketsUserAssigned(userId);
                model.NumberTicketsOwned = bugTrackerHelper.NumberTicketsUserOwned(userId);
                model.NumberTickets = bugTrackerHelper.GetTicketsForDevSubmitters(userId).Count();
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ManageUsers()
        {
            var model = new ManageUsersViewModel();

            model.AllRoles = bugTrackerHelper.GetAllRolesNames();
            model.Users = bugTrackerHelper.GetAllUsers().Select(user => new UserViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                UserRoles = UserManager.GetRoles(user.Id).ToList()

            }).ToList();

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult EditUserRoles(string id)
        {
            var model = new EditUserRolesViewModel();
            var user = bugTrackerHelper.GetUserById(id);

            model.UserId = id;
            model.DisplayName = user.DisplayName;
            model.UserName = user.UserName;

            var allRoles = bugTrackerHelper.GetAllRolesNames();
            var userRolesId = user.Roles.Select(role => role.RoleId).ToList();

            for (var i = 0; i < DbContext.Roles.Count(); i++)
            {
                Role role = new Role();

                role.RoleName = allRoles[i];

                if (UserManager.IsInRole(id, allRoles[i]))
                {
                    role.IsChecked = true;
                }
                else
                {
                    role.IsChecked = false;
                }
                model.UserRoles.Add(role);
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult EditUserRoles(string id, EditUserRolesViewModel formData)
        {
            var user = bugTrackerHelper.GetUserById(id);
            var currentUserId = User.Identity.GetUserId();

            foreach (var role in formData.UserRoles)
            {
                if (role.IsChecked && !UserManager.IsInRole(user.Id, role.RoleName))
                {
                    UserManager.AddToRole(user.Id, role.RoleName);
                }
                else if (!role.IsChecked && UserManager.IsInRole(user.Id, role.RoleName))
                {
                    if (!(currentUserId == id && role.RoleName == "Admin"))
                    {
                        UserManager.RemoveFromRole(user.Id, role.RoleName);
                    }
                }
            }
            // If admin edit his roles re-assign his account
            if (id == currentUserId)
            {
                var authenticationManager = HttpContext.GetOwinContext().Authentication;
                var identity = UserManager.CreateIdentity(user, DefaultAuthenticationTypes.ApplicationCookie);

                authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);
            }

            return RedirectToAction(nameof(HomeController.ManageUsers));
        }

    }
}