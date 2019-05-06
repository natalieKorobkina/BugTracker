using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Domain
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string Description { get; set; }
        public bool Archived { get; set; }

        public virtual List<ApplicationUser> Users { get; set; }
        public virtual List<Ticket> Tickets { get; set; }
    }
}