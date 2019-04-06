using BugTracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace BugTracker.Models.Helpers
{
    public class BugTrackerHelper
    {
        private ApplicationDbContext DbContext;

        public BugTrackerHelper(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public List<string> GetAllRolesNames()
        {
            return DbContext.Roles.Select(role => role.Name).ToList();
        }

        public List<string> GetAllUserRolesNames()
        {
            return DbContext.Roles.Select(role => role.Name).ToList();
        }

        public List<ApplicationUser> GetAllUsers()
        {
            return DbContext.Users.ToList();
        }

        public ApplicationUser GetUserById(string id)
        {
            return DbContext.Users.Where(p => p.Id == id).FirstOrDefault();
        }

        public List<ApplicationUser> GetProjectUsersById(int? id)
        {
            return DbContext.Projects.Where(p => p.Id == id).SelectMany(p => p.Users).ToList();
        }

        public List<Project> GetUserProjectsById(string id)
        {
            return DbContext.Users.Where(u => u.Id == id).SelectMany(u => u.Projects).ToList(); 
        }

        public Project GetProjectById(int? id)
        {
            return DbContext.Projects.Where(p => p.Id == id).FirstOrDefault();
        }

        public string GetProjectNameById(int? id)
        {
            return DbContext.Projects.Where(p => p.Id == id).Select(p => p.Name).FirstOrDefault();
        }

        public string GetStringFromList(List<string> list)
        {
            return Regex.Replace(string.Join(", ", list), "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
        }

    }
}