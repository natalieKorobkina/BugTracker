using AutoMapper;
using BugTracker.Models;
using BugTracker.Models.Domain;
using BugTracker.Models.Filters;
using BugTracker.Models.Helpers;
using BugTracker.Models.ViewModels.Attachment;
using Microsoft.AspNet.Identity;
using System;
using System.IO;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class AttachmentController : Controller
    {
        private ApplicationDbContext DbContext;
        private BugTrackerHelper bugTrackerHelper;

        public AttachmentController()
        {
            DbContext = new ApplicationDbContext();
            bugTrackerHelper = new BugTrackerHelper(DbContext);
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
        public ActionResult CreateAttachment(int? id, CreateEditAttachmentViewModel formData)
        {
            return SaveAttachment(id, null, formData);
        }
        
        [HttpGet]
        [Authorize]
        [HasRightEdit]
        public ActionResult EditAttachment(int? id, int? attachmentId)
        {
            var model = new CreateEditAttachmentViewModel();
            var attachment = bugTrackerHelper.GetAttachmentById(attachmentId.Value);

            model = Mapper.Map<CreateEditAttachmentViewModel>(attachment);

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [HasRightEdit]
        public ActionResult EditAttachment(int? id, int? attachmentId, CreateEditAttachmentViewModel formData)
        {
            return SaveAttachment(id, attachmentId, formData);
        }

        private ActionResult SaveAttachment(int? id, int? attachmentId, CreateEditAttachmentViewModel formData)
        {
            if (formData.Media == null && attachmentId == null)
            {
                ModelState.AddModelError("FileURL", "Please upload file");
                return View();
            }

            if (!ModelState.IsValid || !id.HasValue)
                return RedirectToAction("AllTickets", "Ticket");

            TicketAttachment attachment = new TicketAttachment();

            if (attachmentId == null)
            {
                attachment.DateCreated = DateTime.Now;
                attachment.TicketId = id.Value;
                attachment.UserId = User.Identity.GetUserId();
                DbContext.TicketAttachments.Add(attachment);
            }
            else
            {
                attachment = bugTrackerHelper.GetAttachmentById(attachmentId.Value);
            }

            attachment.Description = formData.Description;
            FileUpload(attachment, formData);
            DbContext.SaveChanges();

            return RedirectToAction("TicketDetails", "Ticket", new { id = attachment.TicketId });
        }

        private void FileUpload(TicketAttachment attachment, CreateEditAttachmentViewModel formData)
        {
            string fileExtension;

            if (formData.Media != null)
            {
                fileExtension = Path.GetExtension(formData.Media.FileName).ToLower();
                //Create directory if it doesn't exists
                if (!Directory.Exists(AttachmentConstants.MappedUploadFolder))
                {
                    Directory.CreateDirectory(AttachmentConstants.MappedUploadFolder);
                }
                //Get file name with Guid to prevent name repetition 
                //and calculate full path with upload folder which is in constants
                var fileName = Guid.NewGuid().ToString() + fileExtension;
                attachment.FileName = fileName;
                var fullPathWithName = AttachmentConstants.MappedUploadFolder + fileName;
                //Actual save on hard disk
                formData.Media.SaveAs(fullPathWithName);
                attachment.FilePath = fullPathWithName;
                attachment.FileUrl = AttachmentConstants.UploadFolder + fileName;
            }
        }

        [HttpPost]
        [Authorize]
        [HasRightEdit]
        public ActionResult Delete(int? attachmentId)
        {
            if (attachmentId.HasValue)
            {
                var attachment = bugTrackerHelper.GetAttachmentById(attachmentId);

                if (attachment != null)
                {
                    DbContext.TicketAttachments.Remove(attachment);
                    DbContext.SaveChanges();
                }

                return RedirectToAction("TicketDetails", "Ticket", new { id = attachment.TicketId });
            }

            return RedirectToAction("Alltickets", "Ticket");
        }
    }
}