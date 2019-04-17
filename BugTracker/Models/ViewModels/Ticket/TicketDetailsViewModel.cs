using BugTracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels
{
    public class TicketDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }

        public Project Project { get; set; }
        public TicketType TicketType { get; set; }
        public TicketPriority TicketPriority { get; set; }
        public TicketStatus TicketStatus { get; set; }
        public ApplicationUser OwnerUser { get; set; }
        public ApplicationUser AssignedToUser { get; set; }
        public List<TicketAttachment> Attachments { get; set; }
        public List<TicketComment> Comments { get; set; }

        public bool CanCreate { get; set; }
    }
}