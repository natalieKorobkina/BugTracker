using BugTracker.Controllers;
using BugTracker.Models.Domain;
//using BugTracker.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using BugTracker.Models.ViewModels;

namespace BugTracker.Models.Helpers
{
    public class BugTrackerHelper
    {
        private ApplicationDbContext DbContext;

        public BugTrackerHelper(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
        }

        //Helpers for Projects
        public IQueryable<Project> ActiveProjects()
        {
            return DbContext.Projects.Where(p => p.Archived == false);
        }

        public List<ApplicationUser> GetProjectUsersById(int? id)
        {
            return ActiveProjects().Where(p => p.Id == id).SelectMany(p => p.Users).ToList();
        }

        public int GetNumberProject()
        {
            return ActiveProjects().ToList().Count();
        }

        public List<Project> GetUserProjectsById(string id)
        {
            return DbContext.Users.Where(u => u.Id == id).SelectMany(u => u.Projects.Where(p => p.Archived == false)).ToList();
        }

        public Project GetProjectById(int? id)
        {
            return ActiveProjects().Where(p => p.Id == id).FirstOrDefault();
        }

        public string GetProjectNameById(int id)
        {
            return ActiveProjects().Where(p => p.Id == id).Select(p => p.Name).FirstOrDefault();
        }

        public List<int> AllIdOfUserProjects(string userId)
        {
            return DbContext.Users.Where(u => u.Id == userId)
                .SelectMany(u => u.Projects.Where(p => p.Archived == false)).Select(p => p.Id).ToList();
        }

        //Helpers for Tickets
        public IQueryable<Ticket> ActiveTickets()
        {
            return DbContext.Tickets.Where(t => t.Archived == false);
        }

        public int GetListTicketsByStatus(string statusName)
        {
            return ActiveTickets().Where(p => p.TicketStatus.Name == statusName).Count();
        }

        public IQueryable<Ticket> GetTicketsForSubmitters(string userId)
        {
            //Tickets of all assigned projects and tickets submitter is owned 
            var allIdOfUserProjects = AllIdOfUserProjects(userId);

            return ActiveTickets().Where(p => allIdOfUserProjects.Contains(p.ProjectId) || p.OwnerUserId == userId);
        }

        public IQueryable<Ticket> GetTicketsForDeveloper(string userId)
        {
            //Tickets of all assigned projects and tickets they are assigned
            var allIdOfUserProjects = AllIdOfUserProjects(userId);

            return ActiveTickets().Where(p => allIdOfUserProjects.Contains(p.ProjectId)
                || p.AssignedToUserId == userId);
        }

        public int NumberTicketsUserAssigned(string userId)
        {
            return ActiveTickets().Where(p => p.AssignedToUserId == userId).Count();
        }

        public int NumberTicketsUserOwned(string userId)
        {
            return ActiveTickets().Where(p => p.OwnerUserId == userId).Count();
        }

        public IQueryable<Ticket> GetTicketsForDevSubmitters(string userId)
        {
            //Tickets of all assigned projects and tickets they are assigned and owned
            var allIdOfUserProjects = AllIdOfUserProjects(userId);

            return ActiveTickets().Where(p => allIdOfUserProjects.Contains(p.ProjectId)
                || p.AssignedToUserId == userId || p.OwnerUserId == userId);
        }

        public Ticket GetCurrentTicketById(int id)
        {
            return ActiveTickets().Where(p => p.Id == id).FirstOrDefault();
        }

        //Helpers for Users and Roles
        public List<string> GetAllRolesNames()
        {
            return DbContext.Roles.Select(role => role.Name).ToList();
        }

        public List<string> GetAllUserRolesNames()
        {
            return DbContext.Roles.Select(role => role.Name).ToList();
        }

        public List<ApplicationUser> GetAllUsers()
        {
            return DbContext.Users.ToList();
        }

        public ApplicationUser GetUserById(string id)
        {
            return DbContext.Users.Where(p => p.Id == id).FirstOrDefault();
        }

        public bool IsAssigned(AllTicketsViewModel ticket, string userId)
        {
            return ((ticket.AssignedToUser != null) && (userId == ticket.AssignedToUser.Id));
        }

        public bool IsOwned(AllTicketsViewModel ticket, string userId)
        {
            return (userId == ticket.OwnerUser.Id);
        }

        public IEnumerable<SelectListItem> GetAllDevelopers()
        {
            var developerRoleId = DbContext.Roles.Where(r => r.Name == "Developer").Select(p => p.Id).FirstOrDefault();

            return DbContext.Users.Where(x => x.Roles.Select(y => y.RoleId).Contains(developerRoleId))
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.DisplayName

                }).ToList();
        }

        public string GetUserId()
        {
            return HttpContext.Current.User.Identity.GetUserId();
        }

        public bool CheckIfUserAdminManager()
        {
            return HttpContext.Current.User.IsInRole("Admin")
                || HttpContext.Current.User.IsInRole("ProjectManager");
        }

        //Others
        public string GetStringFromList(List<string> list)
        {
            return Regex.Replace(string.Join(", ", list), "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
        }

        public TicketAttachment GetAttachmentById(int? id)
        {
            return DbContext.TicketAttachments.Where(a => a.Id == id.Value).Select(a => a).FirstOrDefault();
        }

        public TicketComment GetCommentById(int? id)
        {
            return DbContext.TicketComments.FirstOrDefault(c => c.Id == id.Value);
        }

        public int GetStatusOpen()
        {
            return DbContext.TicketStatuses.Where(p => p.Name == "Open").Select(p => p.Id).FirstOrDefault();
        }

        public List<AttachmentForList> GetListAttachments(Ticket ticket)
        {
            return ticket.Attachments.Select(p => new AttachmentForList
            {
                TicketAttachment = p,
                CanEdit = CheckIfUserAdminManager() || (p.UserId == GetUserId())
            }).ToList();
        }

        public List<CommentForList> GetListComments(Ticket ticket)
        {
            return ticket.Comments.Select(p => new CommentForList
            {
                TicketComment = p,
                CanEdit = CheckIfUserAdminManager() || (p.UserId == GetUserId())
            }).ToList();
        }

        public List<TicketHistory> GetListHistories(Ticket ticket)
        {
            return ticket.Histories.Select(p => p).ToList();
        }

        // Helpers for DropDownLists
        public IEnumerable<SelectListItem> GetDropDownListProjects() 
        {
            return ActiveProjects().Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList();
        }

        public IEnumerable<SelectListItem> GetDropDownListUsersProjects(string usedId)
        {
            var userProjectsById = GetUserProjectsById(usedId);

            return userProjectsById.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList();
        }

        public IEnumerable<SelectListItem> GetDropDownListUsersProjectsEdit(string usedId, int ticketProjectId)
        {
            //List with project id from current ticket, in case if user not assigned to ticket's project but assigned to that ticket
            var userProjectsById = GetUserProjectsById(usedId);
            var isInList = userProjectsById.Where(p => p.Id == ticketProjectId).Select(p => p).Any();
            if (!isInList)
            {
                var currentTicketProject = ActiveProjects().Where(p => p.Id == ticketProjectId).FirstOrDefault();
                userProjectsById.Add(currentTicketProject);
            }

            return userProjectsById.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList();
        }

        public IEnumerable<SelectListItem> GetDropDownListTypes()
        {
            return DbContext.TicketTypes.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList();
        }

        public IEnumerable<SelectListItem> GetDropDownListPriorities()
        {
            return DbContext.TicketPriorities.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList();
        }

        public IEnumerable<SelectListItem> GetDropDownListStatuses()
        {
            return DbContext.TicketStatuses.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList();
        }
    }
}