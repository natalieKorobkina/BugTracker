using BugTracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels
{
    public class AllTicketsViewModel
    {
        public int Id { get; set; }
        public Project Project { get; set; }
        public string Title { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public TicketType TicketType { get; set; }
        public TicketStatus TicketStatus { get; set; }
        public TicketPriority TicketPriority { get; set; }
        public ApplicationUser OwnerUser { get; set; }
        public ApplicationUser AssignedToUser { get; set; }
        public bool EditAvailable { get; set; }
        public string UserId { get; set; }
    }
}