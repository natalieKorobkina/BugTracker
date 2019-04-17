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

namespace BugTracker.Models.Helpers
{
    public class BugTrackerHelper
    {
        private ApplicationDbContext DbContext;

        public BugTrackerHelper(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
        }

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

        public List<ApplicationUser> GetProjectUsersById(int? id)
        {
            return DbContext.Projects.Where(p => p.Id == id).SelectMany(p => p.Users).ToList();
        }

        public List<Project> GetUserProjectsById(string id)
        {
            return DbContext.Users.Where(u => u.Id == id).SelectMany(u => u.Projects).ToList(); 
        }

        public Project GetProjectById(int? id)
        {
            return DbContext.Projects.Where(p => p.Id == id).FirstOrDefault();
        }

        public string GetProjectNameById(int id)
        {
            return DbContext.Projects.Where(p => p.Id == id).Select(p => p.Name).FirstOrDefault();
        }

        public string GetStringFromList(List<string> list)
        {
            return Regex.Replace(string.Join(", ", list), "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
        }


        public List<int> AllIdOfUserProjects(string userId)
        {
            return DbContext.Users.Where(u => u.Id == userId)
                .SelectMany(u => u.Projects).Select(p => p.Id).ToList();
        }

        public IQueryable<Ticket> GetTicketsForSubmitters(string userId)
        {
            //Tickets of all assigned projects and tickets submitter is owned 
            var allIdOfUserProjects = AllIdOfUserProjects(userId);

            return DbContext.Tickets.Where(p => allIdOfUserProjects.Contains(p.ProjectId)|| p.OwnerUserId == userId);
        }
        
        public IQueryable<Ticket> GetTicketsForDeveloper(string userId)
        {
            //Tickets of all assigned projects and tickets they are assigned
            var allIdOfUserProjects = AllIdOfUserProjects(userId);

            return DbContext.Tickets.Where(p => allIdOfUserProjects.Contains(p.ProjectId)
                || p.AssignedToUserId == userId);
        }

        public IQueryable<Ticket> GetTicketsForDevSubmitters(string userId)
        {
            //DbContext.Tickets.Where(p => (isAdmin) ||
            //(isDeveloper && p.AssignedToUserId == userId) ||
            //(isSub && p.))

            //Tickets of all assigned projects and tickets they are assigned and owned
            var allIdOfUserProjects = AllIdOfUserProjects(userId);

            return DbContext.Tickets.Where(p => allIdOfUserProjects.Contains(p.ProjectId)
                || p.AssignedToUserId == userId || p.OwnerUserId == userId);
        }

        public bool IsAssigned(ViewModels.AllTicketsViewModel ticket, string userId)
        {
            return ((ticket.AssignedToUser != null) && (userId == ticket.AssignedToUser.Id));
        }

        public bool IsOwned(ViewModels.AllTicketsViewModel ticket, string userId)
        {
            return (userId == ticket.OwnerUser.Id);
        }

        public Ticket GetCurrentTicketById(int id)
        {
            return DbContext.Tickets.Where(p => p.Id == id).FirstOrDefault();
        }

        public IEnumerable<SelectListItem> GetDropDownListProjects() 
        {
            return DbContext.Projects.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList();
        }

        public IEnumerable<SelectListItem> GetDropDownListProjectsCreate(string usedId)
        {
            var userProjectsById = GetUserProjectsById(usedId);

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

        public int GetStatusOpen()
        {
            return DbContext.TicketStatuses.Where(p => p.Name == "Open").Select(p => p.Id).FirstOrDefault();
        }
        
        //public bool CheckIfHasRight(bool isDeveloper, bool isSubmitter, bool isAdminManager, Ticket ticket, string userId)
        //{
        //    if (!isAdminManager)
        //    {
        //        if (isDeveloper && (ticket.AssignedToUserId != userId))
        //            return true;

        //        if (isSubmitter && (ticket.OwnerUserId != userId))
        //            return true;
        //    }

        //    return false;
        //}

        //public bool UserCanCreate(bool isDeveloper, bool isSubmitter, bool isAdminManager, Ticket ticket, string userId)
        //{
        //    if (isAdminManager)
        //        return true;

        //    if (isDeveloper && isSubmitter)
        //        return (userId == ticket.AssignedToUserId || userId == ticket.OwnerUserId);

        //    if (isDeveloper)
        //        return userId == ticket.AssignedToUserId;

        //    return userId == ticket.OwnerUserId;
        //}
    }
}