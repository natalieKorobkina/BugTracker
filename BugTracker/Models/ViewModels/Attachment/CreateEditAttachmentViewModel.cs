using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels.Attachment
{
    public class CreateEditAttachmentViewModel
    {
        public string FilePath { get; set; }

        [Required(ErrorMessage = "Please, enter attachment's description.")]
        public string Description { get; set; }
        public string FileUrl { get; set; }
        public string FileName { get; set; }

        public HttpPostedFileBase Media { get; set; }
        public string MediaUrl { get; set; }
    }
}