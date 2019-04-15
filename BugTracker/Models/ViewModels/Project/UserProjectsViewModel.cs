using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels
{
    public class UserProjectsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public int MembersNumber { get; set; }
        public int TicketsNumber { get; set; }
    }
}