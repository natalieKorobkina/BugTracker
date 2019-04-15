using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.ViewModels.Attachment
{
    public class CreateAttachmentViewModel
    {
        public string FilePath { get; set; }
        public string Discription { get; set; }
        public string FileUrl { get; set; }

        public HttpPostedFileBase Media { get; set; }
        public string MediaUrl { get; set; }
    }
}