using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels
{
    public class CreateCommentViewModel
    {
        [Required(ErrorMessage = "Please, enter comment.")]
        public string Comment { get; set; }
    }
}