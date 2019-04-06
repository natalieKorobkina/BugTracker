using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class ManageUsersViewModel
    {
        public List<UserViewModel> Users {get; set;}
        public List<string> AllRoles { get; set; }

        public ManageUsersViewModel()
        {
            Users = new List<UserViewModel>();
        }
    }

    public class UserViewModel
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public List<string> UserRoles { get; set; }
    }
}