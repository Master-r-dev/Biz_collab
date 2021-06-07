using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Biz_collab.Models;

namespace Biz_collab.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Role_Power> Role_Powers { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<MutedName> MutedNames { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Client>()
                .HasKey(x => x.Id);
            builder.Entity<Group>()
                .HasKey(x => x.Id);
            builder.Entity<Role_Power>()
                .HasKey(x => new { x.ClientId, x.GroupId });
            builder.Entity<Role_Power>()
                .HasOne(x => x.Client)
                .WithMany(m => m.MyGroups)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Role_Power>()
                .HasOne(x => x.Group)
                .WithMany(m => m.Clients)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Transaction>()
                .HasKey(x => x.Id);
            builder.Entity<Transaction>()
                .HasOne(x => x.Client)
                .WithMany(m=>m.MyTransactions)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Transaction>()
                .HasOne(x => x.Group)
                .WithMany(m => m.Transactions)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Vote>()
                .HasKey(x => new { x.ClientId, x.TransactionId });
            builder.Entity<Vote>()
                .HasOne(x => x.Client)
                .WithMany(m => m.MyVotes)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Vote>()
                .HasOne(x => x.Transaction)
                .WithMany(m => m.Votes)
                .HasForeignKey(x => x.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Message>()
                .HasKey(x => x.Id);
            builder.Entity<Message>()
                .HasOne(x => x.Client)
                .WithMany(m => m.MyMessages)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Message>()
                .HasOne(x => x.Group)
                .WithMany(m => m.Messages)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Notification>()
                .HasKey(x => x.Id);
            builder.Entity<Notification>()
                .HasOne(x => x.Client)
                .WithMany(m => m.MyNotifications)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity < MutedName > ()
                .HasKey(x => x.Id);
            builder.Entity<MutedName>()
                .HasOne(x => x.Client)
                .WithMany(m => m.MutedName)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(builder);
        }
    }
}
