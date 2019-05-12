namespace BugTracker.Migrations
{
    using BugTracker.Models;
    using BugTracker.Models.Domain;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BugTracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(BugTracker.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
            // Seeding Tickets' Types
            foreach (var type in ProjectConstants.AllowedTicketTypes)
            {
                TicketType ticketType = new TicketType();
                ticketType.Name = type;
                context.TicketTypes.AddOrUpdate(p => p.Name, ticketType);
            }
            // Seeding Tickets' Statuses
            foreach (var status in ProjectConstants.AllowedTicketStatuses)
            {
                TicketStatus ticketStatus = new TicketStatus();
                ticketStatus.Name = status;
                context.TicketStatuses.AddOrUpdate(p => p.Name, ticketStatus);
            }
            // Seeding Tickets' Priorities
            foreach (var priority in ProjectConstants.AllowedTicketPriorities)
            {
                TicketPriority ticketPriority = new TicketPriority();
                ticketPriority.Name = priority;
                context.TicketPriorities.AddOrUpdate(p => p.Name, ticketPriority);
            }
            // Seeding Users' Roles
            var Roles = SeededRoles.CreateRolesList();
            var Accounts = SeededRoles.CreateAccountsList();

            foreach (var role in Roles)
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

                if (!context.Roles.Any(p => p.Name == role))
                {
                    var roleToAdd = new IdentityRole(role);
                    roleManager.Create(roleToAdd);
                }
            }
            // Seeding Users
            foreach (var account in Accounts)
            {
                //Creating the user with their role
                var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));

                ApplicationUser userToCreate;

                if (!context.Users.Any(p => p.Email == account.Email))
                {
                    userToCreate = new ApplicationUser();
                    userToCreate.UserName = account.Email;
                    userToCreate.Email = account.Email;
                    userToCreate.DisplayName = account.DisplayName;

                    userManager.Create(userToCreate, account.Password);
                }
                else
                {
                    userToCreate = context.Users.First(p => p.UserName == account.Email);
                }
                //Make sure the user is on its role
                if (!userManager.IsInRole(userToCreate.Id, account.Role))
                {
                    userManager.AddToRole(userToCreate.Id, account.Role);
                }
            }

            context.SaveChanges();
        
    }
    }
}
