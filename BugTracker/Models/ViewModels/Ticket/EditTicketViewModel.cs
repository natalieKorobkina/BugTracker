using BugTracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models.ViewModels
{
    public class EditTicketViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please, enter ticket's title.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please, enter ticket's description.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please, enter ticket's project name.")]
        public int ProjectId { get; set; }
        public IEnumerable<SelectListItem> Projects { get; set; }

        [Required(ErrorMessage = "Please, enter ticket's type.")]
        public int TicketTypeId { get; set; }
        public IEnumerable<SelectListItem> Types { get; set; }

        [Required(ErrorMessage = "Please, enter ticket's priority.")]
        public int TicketPriorityId { get; set; }
        public IEnumerable<SelectListItem> Priorities { get; set; }

        [Required(ErrorMessage = "Please, enter ticket's status.")]
        public int TicketStatusId { get; set; }
        public IEnumerable<SelectListItem> Statuses { get; set; }

        public DateTime DateCreated { get; set; }
    }
}