using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels
{
    public class CreateUpdateProjectViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please, enter project's name.")]
        public string Name { get; set; }
        public string Discription { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}