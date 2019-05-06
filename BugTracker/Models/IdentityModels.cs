using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using BugTracker.Models.Domain;
using BugTracker.Models.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BugTracker.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public virtual List<Project> Projects { get; set; }
        public string DisplayName { get; set; }

        [InverseProperty(nameof(Ticket.OwnerUser))]
        public virtual List<Ticket> CreatedTickets { get; set; }

        [InverseProperty(nameof(Ticket.AssignedToUser))]
        public virtual List<Ticket> AssignedTickets { get; set; }

        public virtual List<TicketComment> Comments { get; set; }
        public virtual List<TicketAttachment> Attachments { get; set; }
        public virtual List<TicketHistory> Histories { get; set; }
        public virtual List<TicketNotification> Notifications { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Project> Projects { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        public DbSet<TicketStatus> TicketStatuses { get; set; }
        public DbSet<TicketPriority> TicketPriorities { get; set; }
        public DbSet<TicketAttachment> TicketAttachments { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketNotification> TicketNotifications { get; set; }
        public DbSet<TicketHistory> TicketHistories { get; set; }
        public DbSet<ExceptionLog> ExceptionLogs { get; set; }

        private HistoryHelper historyHelper;
        private NotificationHelper notificationHelper;

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Project>()
                .Property(p => p.Name).IsRequired().HasMaxLength(100).IsUnicode(false);
            modelBuilder.Entity<Project>()
                .Property(p => p.Description).IsRequired().HasMaxLength(300).IsUnicode(false);

            modelBuilder.Entity<Ticket>()
                .Property(p => p.Title).IsRequired().HasMaxLength(100).IsUnicode(false);
            modelBuilder.Entity<Ticket>()
                .Property(p => p.Description).IsRequired().HasMaxLength(300).IsUnicode(false);

            modelBuilder.Entity<TicketAttachment>()
                .Property(p => p.FileName).IsRequired().HasMaxLength(100).IsUnicode(false);
            modelBuilder.Entity<TicketAttachment>()
                .Property(p => p.FilePath).IsRequired().HasMaxLength(300).IsUnicode(false);
            modelBuilder.Entity<TicketAttachment>()
                .Property(p => p.Description).IsRequired().HasMaxLength(300).IsUnicode(false);
            modelBuilder.Entity<TicketAttachment>()
                .Property(p => p.FileUrl).IsRequired().HasMaxLength(300).IsUnicode(false);

            modelBuilder.Entity<TicketComment>()
               .Property(p => p.Comment).IsRequired().HasMaxLength(300).IsUnicode(false);

            modelBuilder.Entity<TicketHistory>()
               .Property(p => p.Property).IsRequired().HasMaxLength(30).IsUnicode(false);
            modelBuilder.Entity<TicketHistory>()
                .Property(p => p.OldValue).IsRequired().HasMaxLength(300).IsUnicode(false);
            modelBuilder.Entity<TicketHistory>()
                .Property(p => p.NewValue).IsRequired().HasMaxLength(300).IsUnicode(false);

            modelBuilder.Entity<TicketPriority>()
                .Property(p => p.Name).IsRequired().HasMaxLength(50).IsUnicode(false);

            modelBuilder.Entity<TicketStatus>()
                .Property(p => p.Name).IsRequired().HasMaxLength(50).IsUnicode(false);

            modelBuilder.Entity<TicketType>()
                .Property(p => p.Name).IsRequired().HasMaxLength(50).IsUnicode(false);
        }

        object GetPrimaryKeyValue(DbEntityEntry entry)
        {
            var objectStateEntry = ((IObjectContextAdapter)this).ObjectContext.ObjectStateManager.GetObjectStateEntry(entry.Entity);
            return objectStateEntry.EntityKey.EntityKeyValues[0].Value;
        }

        public override int SaveChanges()
        {
            var modifiedEntities = ChangeTracker.Entries().Where(p => p.State == EntityState.Modified).ToList();
            bool wasModifyed = false;
            Ticket ticket = new Ticket();

            foreach (var change in modifiedEntities)
            {
                var entityName = change.Entity.GetType().Name;
                var primaryKey = Convert.ToInt32(GetPrimaryKeyValue(change).ToString());
                ticket = Tickets.FirstOrDefault(t => t.Id == primaryKey);

                foreach (var prop in change.OriginalValues.PropertyNames)
                {
                    if (Enum.IsDefined(typeof(PropertyNames), prop))
                    {
                        var originalValue = "not assigned";
                        var currentValue = "not assigned";

                        if (change.OriginalValues[prop] != null) originalValue = change.OriginalValues[prop].ToString();
                        if (change.CurrentValues[prop] != null) currentValue = change.CurrentValues[prop].ToString();

                        if (originalValue != currentValue)
                        {
                            historyHelper = new HistoryHelper(Create());
                            historyHelper.CreatePropertyHistory(primaryKey, prop, originalValue, currentValue);
                            wasModifyed = true;
                        }
                    }
                }
                notificationHelper = new NotificationHelper(Create());

                if (wasModifyed)
                {
                    var message = notificationHelper.CreateModificationNotification(ticket.Title);

                    notificationHelper.SendNotification(ticket, message, false);
                }
            }
            return base.SaveChanges();
        }

        public enum PropertyNames
        {
            Description,
            Title,
            ProjectId,
            TicketTypeId,
            TicketPriorityId,
            TicketStatusId
        }
    }
}