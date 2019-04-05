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
    public class ProjectController : Controller
    {
        private ApplicationDbContext DbContext;

        public ProjectController()
        {
            DbContext = new ApplicationDbContext();
        }

        public ActionResult Index()
        {
            var model = DbContext.Projects.Select(p => new AllProjectsViewModel
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

        public ActionResult UserProjects()
        {
            var userId = User.Identity.GetUserId();
            var query = DbContext.Users.Where(u => u.Id == userId).SelectMany(u => u.Projects);
            var model = query.Select(p => new UserProjectsViewModel
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

        public ActionResult EditMembers(int? projectId)
        {
            var usersInProject = DbContext.Projects.Where(p => p.Id == projectId).SelectMany(p => p.Users);
            var users = DbContext.Projects.Where(p => p.Id == projectId).SelectMany(p => p.Users).ToList();
            //var query = from user in DbContext.Users
            //            where user.Projects.Any(p => p.Id != projectId)
            //            select user;
            //var query2 = DbContext.Users.Where(p => p.Projects.Any(t => t.Id != projectId));
            var allUsers = DbContext.Users.ToList();
            //var result = allUsers.Select(t => t.).Except(usersInProject);
            var model = new EditMembersViewModel();
            model.ProjectMembers = usersInProject.Select(p => new User
            {
                ProjectId = projectId,
                UserId = p.Id,
                UserDisplayName = p.DisplayName
               // UserRoles = string.Join(",", p.Roles.Select(role => role.RoleId))

            }).ToList();

            model.NotMembers = allUsers.Where(u1 => !users.Any(u2 => u2.Id == u1.Id)).Select(p => new User
            {
                ProjectId = projectId,
                UserId = p.Id,
                UserDisplayName = p.DisplayName
                // UserRoles = string.Join(",", p.Roles.Select(role => role.RoleId))

            }).ToList();

            return View(model);
        }

        public ActionResult AddUser (int? projectId, string userId)
        {
            var project = DbContext.Projects.Where(p => p.Id == projectId).FirstOrDefault();
            var user = DbContext.Users.Where(u => u.Id == userId).FirstOrDefault();
            project.Users.Add(user);
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
            ViewBag.Message = "Add New Post";

            return AddProjectToDatabase(null, formData);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult Edit(int? id)
        {
            if (id.HasValue)
            {
                var project = DbContext.Projects.FirstOrDefault(p => p.Id == id.Value);

                if (project != null)
                {
                    var model = new CreateUpdateProjectViewModel();

                    model.Name = project.Name;
                    model.Discription = project.Discription;

                    return View(model);
                }
            }
            return RedirectToAction(nameof(ProjectController.Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult Edit(int? id, CreateUpdateProjectViewModel formData)
        {
            return AddProjectToDatabase(id, formData);
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
                project = DbContext.Projects.FirstOrDefault(p => p.Id == id);

                if (project == null)
                {
                    return RedirectToAction(nameof(ProjectController.Index));
                }
            }
            project.Name = formData.Name;
            project.Discription = formData.Discription;
            project.DateUpdated = DateTime.Now;

            DbContext.SaveChanges();

            return RedirectToAction(nameof(ProjectController.Index)); ;
        }
    }
}