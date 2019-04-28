using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class SeededRoles
    {
        public string Role { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        public SeededRoles(Roles role, string displayName, string password, string email)
        {
            Role = role.ToString();
            DisplayName = displayName;
            Password = password;
            Email = email;
        }

        public static List<string> CreateRolesList()
        {
            List<string> RolesToAdd = new List<string>()
            {
                Roles.Admin.ToString(),
                Roles.ProjectManager.ToString(),
                Roles.Developer.ToString(),
                Roles.Submitter.ToString()
            };

            return RolesToAdd;
        }

        public static List<SeededRoles> CreateAccountsList()
        {
            List<SeededRoles> Accounts = new List<SeededRoles>()
            {
                new SeededRoles(Roles.Admin,"admin","Password-1", "admin@mybugtracker.com"),
                new SeededRoles(Roles.ProjectManager, "project manager", "Password-1", "manager@mybugtracker.com"),
                new SeededRoles(Roles.Developer, "developer", "Password-1", "developer@mybugtracker.com"),
                new SeededRoles(Roles.Submitter,"submitter","Password-1", "submitter@mybugtracker.com")
            };

            return Accounts;
        }
    }
}