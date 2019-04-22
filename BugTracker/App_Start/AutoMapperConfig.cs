using AutoMapper;
using BugTracker.Models.Domain;
using BugTracker.Models.ViewModels;
using BugTracker.Models.ViewModels.Attachment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.App_Start
{
    public class AutoMapperConfig
    {
        public static void Init()
        {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<Ticket, TicketDetailsViewModel>();
                cfg.CreateMap<Ticket, AllTicketsViewModel>();
                cfg.CreateMap<CreateTicketViewModel, Ticket>();
                cfg.CreateMap<Ticket, EditTicketViewModel>();
                cfg.CreateMap<EditTicketViewModel, Ticket>();
                cfg.CreateMap<TicketAttachment, CreateEditAttachmentViewModel>();
                cfg.CreateMap<TicketComment, CreateEditCommentViewModel>();
            });

        }
    }
}