using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker
{
    public static class AttachmentConstants
    {
        public static readonly string UploadFolder = "/Upload/";

        public static readonly string MappedUploadFolder = HttpContext.Current.Server.MapPath(UploadFolder);
    }
}