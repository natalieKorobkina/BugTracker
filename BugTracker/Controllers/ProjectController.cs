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
    public class ProjectController : Controller
    {
        private ApplicationDbContext DbContext;
        private BugTrackerHelper bugTrackerHelper;
        private UserManager<ApplicationUser> userManager;

        public ProjectController()
        {
            DbContext = new ApplicationDbContext();
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            bugTrackerHelper = new BugTrackerHelper(DbContext);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult AllProjects()
        {
            var model = bugTrackerHelper.ActiveProjects().Select(p => new AllProjectsViewModel
            {
                Id = p.Id,
                Name = p.Name,
                MembersNumber = p.Users.Count(),
                TicketsNumber = p.Tickets.Count(),
                DateCreated = p.DateCreated,
                DateUpdated = p.DateUpdated
            }).ToList();

            return View(model);
        }

        [Authorize]
        public ActionResult UserProjects()
        {
            var userId = User.Identity.GetUserId();
            var model = bugTrackerHelper.GetUserProjectsById(userId).Select(p => new UserProjectsViewModel
            {
                Id = p.Id,
                Name = p.Name,
                MembersNumber = p.Users.Count(),
                TicketsNumber = p.Tickets.Count(),
                DateCreated = p.DateCreated,
                DateUpdated = p.DateUpdated
            }).ToList();

            return View(model);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult EditMembers(int id)
        {
            var usersInProject = bugTrackerHelper.GetProjectUsersById(id);
            var allUsers = bugTrackerHelper.GetAllUsers();
            var model = new EditMembersViewModel();

            model.ProjectId = id;
            model.ProjectName = bugTrackerHelper.GetProjectNameById(id);
            model.ProjectMembers = usersInProject.Select(p => new User
            {
                UserId = p.Id,
                UserDisplayName = p.DisplayName,
                UserRoles = bugTrackerHelper.GetStringFromList(userManager.GetRoles(p.Id).ToList())
                
            }).ToList();

            model.NotMembers = allUsers.Where(u1 => !usersInProject.Any(u2 => u2.Id == u1.Id)).Select(p => new User
            {
                UserId = p.Id,
                UserDisplayName = p.DisplayName,
                UserRoles = bugTrackerHelper.GetStringFromList(userManager.GetRoles(p.Id).ToList())
            }).ToList();

            return View(model);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult AddUser (int? projectId, string userId)
        {
            return ManageUser(projectId, userId, true);
        }

        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult RemoveMember(int? projectId, string userId)
        {
            return ManageUser(projectId, userId, false);
        }

        private ActionResult ManageUser(int? projectId, string userId, bool add)
        {
            if (projectId == null)
                return RedirectToAction(nameof(ProjectController.AllProjects));

            if (userId == null)
                return RedirectToAction(nameof(ProjectController.EditMembers), new { id = projectId });

            var project = bugTrackerHelper.GetProjectById(projectId);
            var user = bugTrackerHelper.GetUserById(userId);

            if (project == null || user == null)
                return RedirectToAction(nameof(ProjectController.AllProjects));

            if (add)
            {
                project.Users.Add(user);
            }
            else
            {
                project.Users.Remove(user);
            }
            
            DbContext.SaveChanges();

            return RedirectToAction(nameof(ProjectController.EditMembers), new { id = projectId });
        }

        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult Create(CreateUpdateProjectViewModel formData)
        {
            return AddProjectToDatabase(null, formData);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult Edit(int? id)
        {
            if (id.HasValue)
            {
                var project = bugTrackerHelper.GetProjectById(id.Value);

                if (project != null)
                {
                    var model = new CreateUpdateProjectViewModel();

                    model.Name = project.Name;
                    model.Discription = project.Description;

                    return View(model);
                }
            }
            return RedirectToAction(nameof(ProjectController.AllProjects));
        }

        [HttpPost]
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult Edit(int? id, CreateUpdateProjectViewModel formData)
        {
            return AddProjectToDatabase(id, formData);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult Archive(int? id)
        {
            if (id.HasValue)
            {
                var project = bugTrackerHelper.GetProjectById(id);

                if (project != null)
                {
                    project.Archived = true;

                    foreach (var ticket in project.Tickets)
                    {
                        ticket.Archived = true;
                        ticket.Attachments.ForEach(a => a.Archived = true);
                        ticket.Comments.ForEach(c => c.Archived = true);
                    }

                    DbContext.SaveChanges();
                }
            }

            return RedirectToAction("AllProjects", "Project");
        }

        private ActionResult AddProjectToDatabase(int? id, CreateUpdateProjectViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            Project project;

            if (!id.HasValue)
            {
                project = new Project();
                project.DateCreated = DateTime.Now;
                DbContext.Projects.Add(project);
            }
            else
            {
                project = bugTrackerHelper.GetProjectById(id);

                if (project == null)
                {
                    return RedirectToAction(nameof(ProjectController.AllProjects));
                }
            }
            project.Name = formData.Name;
            project.Description = formData.Discription;
            project.DateUpdated = DateTime.Now;

            DbContext.SaveChanges();

            return RedirectToAction(nameof(ProjectController.AllProjects)); ;
        }
    }
}