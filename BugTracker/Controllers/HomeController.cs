using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BugTracker.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext DbContext;

        public HomeController()
        {
            DbContext = new ApplicationDbContext();
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ManageUsers()
        {
            //var model = DbContext.Users.Select(p => new ManageUsersViewModel
            //{
            //    UserId = p.Id,
            //    DisplayName = p.DisplayName,
            //    UserName = p.UserName

            //}).ToList();

            var allusers = DbContext.Users.ToList();
            //var users = allusers.Where(x => x.Roles.Select(role => role.Name).Contains("User")).ToList();
            var model = new ManageUsersViewModel();
            model.AllRoles = DbContext.Roles.Select(role => role.Name).ToList();
            model.Users = allusers.Select(user => new UserViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                //UserRoles = user.Roles.Join(DbContext.Roles, role => role.Equals.)
                UserRoles = (from userRole in user.Roles
                             join role in DbContext.Roles on userRole.RoleId equals role.Id
                             select role.Name).ToList()
                
            //UserRoles = DbContext.Roles.Select(p => p.Name).ToList()
            //string.Join(",", user.Roles.Select(role => role.RoleId))

        }).ToList();
            
            return View(model);
        }

        [HttpGet]
        public ActionResult EditUserRoles(string id)
        {
            var model = new EditUserRolesViewModel();
            var user = DbContext.Users.Where(p => p.Id == id).FirstOrDefault();
            model.UserId = id;
            model.DisplayName = user.DisplayName;
            model.UserName = user.UserName;
            var allRoles = DbContext.Roles.Select(p => new { id = p.Id, name = p.Name}).ToList();
            var userRolesId = user.Roles.Select(role => role.RoleId).ToList();
            for (var i = 0; i < DbContext.Roles.Count(); i++)
            {
                Role role = new Role();

                role.RoleName = allRoles[i].name;
                if (userRolesId.Contains(allRoles[i].id))
                {
                    role.IsChecked = true;
                    model.UserRoles.Add(role);
                }
                else
                {
                    role.IsChecked = false;
                    model.UserRoles.Add(role);
                }
                
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult EditUserRoles(string id, EditUserRolesViewModel formData)
        {
            var temp = formData;
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(DbContext));

            var user = DbContext.Users.Where(p => p.Id == id).FirstOrDefault();

            foreach(var role in formData.UserRoles)
            if (role.IsChecked && !userManager.IsInRole(user.Id, role.RoleName))
            {
                userManager.AddToRole(user.Id, role.RoleName);
            }
            else if (!role.IsChecked && userManager.IsInRole(user.Id, role.RoleName))
                {
                    userManager.RemoveFromRole(user.Id, role.RoleName);
                }
            return RedirectToAction(nameof(HomeController.ManageUsers));

        }

    }
}