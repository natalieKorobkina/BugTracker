using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels
{
    public class EditMembersViewModel
    {
        public List<User> ProjectMembers { get; set; }
        public List<User> NotMembers { get; set; }

        public EditMembersViewModel()
        {
            ProjectMembers = new List<User>();
            NotMembers = new List<User>();
        }
    }

    public class User
    {
        public int? ProjectId { get; set; }
        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        public string UserRoles { get; set; }
    }
}