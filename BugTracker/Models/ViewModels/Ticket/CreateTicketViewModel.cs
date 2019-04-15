using BugTracker.Models.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models.ViewModels
{
    public class CreateTicketViewModel
    {
        [Required(ErrorMessage = "Please, enter ticket's title.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please, enter ticket's description.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please, enter ticket's project name.")]
        public int ProjectId { get; set; }
        public IEnumerable<SelectListItem> Projects { get; set; }

        [Required(ErrorMessage = "Please, enter ticket's type.")]
        public int TypeId { get; set; }
        public IEnumerable<SelectListItem> Types { get; set; }

        [Required(ErrorMessage = "Please, enter ticket's priority.")]
        public int PriorityId { get; set; }
        public IEnumerable<SelectListItem> Priorities { get; set; }
    }
}