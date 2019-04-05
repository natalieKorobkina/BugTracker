namespace BugTracker.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using BugTracker.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    internal sealed class Configuration : DbMigrationsConfiguration<BugTracker.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "BugTracker.Models.ApplicationDbContext";
        }

        protected override void Seed(BugTracker.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.

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

            foreach (var account in Accounts)
            {
                //Creating the user with their role
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

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
