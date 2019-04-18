using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.Filters;
using BugTracker.Models.ViewModels.Attachment;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class AttachmentController : Controller
    {
        private ApplicationDbContext DbContext;

        public AttachmentController()
        {
            DbContext = new ApplicationDbContext();
        }
        
        [HttpGet]
        [Authorize]
        [HasRightsCheckFilter()]
        public ActionResult CreateAttachment(int? id)
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [HasRightsCheckFilter()]
        public ActionResult CreateAttachment(int? id, CreateAttachmentViewModel formData)
        {
            if (formData.Media == null)
            {
                ModelState.AddModelError("FileURL", "Please upload file");
                return View();
            }

            if (!ModelState.IsValid || !id.HasValue)
                return RedirectToAction("AllTickets", "Ticket");

            TicketAttachment attachment = new TicketAttachment();

            attachment.Description = formData.Description;
            attachment.DateCreated = DateTime.Now;
            attachment.TicketId = id.Value;
            attachment.UserId = User.Identity.GetUserId();

            return FileUpload(attachment, formData);
        }

        private ActionResult FileUpload(TicketAttachment attachment, CreateAttachmentViewModel formData)
        {
            //Handling file upload 
            string fileExtension;

            if (formData.Media != null)
            {
                fileExtension = Path.GetExtension(formData.Media.FileName).ToLower();

                //Create directory if it doesn't exists
                if (!Directory.Exists(AttachmentConstants.MappedUploadFolder))
                {
                    Directory.CreateDirectory(AttachmentConstants.MappedUploadFolder);
                }

                //Get file name with special method and calculate full path with upload folder which is in constants
                var fileName = Guid.NewGuid().ToString() + fileExtension;
                attachment.FileName = fileName;
                var fullPathWithName = AttachmentConstants.MappedUploadFolder + fileName;
                //Actual save on hard disk
                formData.Media.SaveAs(fullPathWithName);
                attachment.FilePath = fullPathWithName;
                attachment.FileUrl = AttachmentConstants.UploadFolder + fileName;
            }
            DbContext.TicketAttachments.Add(attachment);
            DbContext.SaveChanges();

            return RedirectToAction("TicketDetails", "Ticket", new { id = attachment.TicketId}); ;
        }
    }
}