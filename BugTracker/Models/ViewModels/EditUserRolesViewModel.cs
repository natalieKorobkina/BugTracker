using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class EditUserRolesViewModel
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public List<Role> UserRoles { get; set; }

        public EditUserRolesViewModel()
        {
            UserRoles = new List<Role>();
        }
    }

    public class Role
    {
        public string RoleName { get; set; }
        public bool IsChecked { get; set; }
    }
}