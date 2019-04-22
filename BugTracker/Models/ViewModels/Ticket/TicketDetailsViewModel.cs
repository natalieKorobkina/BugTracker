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
        public List<AttachmentForList> TicketAttachments { get; set; }
        public List<CommentForList> TicketComments { get; set; }

        public bool CanCreate { get; set; }
    }

    public class AttachmentForList
    {
        public TicketAttachment TicketAttachment { get; set; }
        public bool CanEdit { get; set; }
    }

    public class CommentForList
    {
        public TicketComment TicketComment { get; set; }
        public bool CanEdit { get; set; }
    }
}