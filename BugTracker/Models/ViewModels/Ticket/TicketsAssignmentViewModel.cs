using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models.ViewModels
{
    public class TicketsAssignmentViewModel
    {
        [Required(ErrorMessage = "Please, enter ticket's Developer name.")]
        public string DeveloperId { get; set; }
        public IEnumerable<SelectListItem> Developers { get; set; }
    }
}