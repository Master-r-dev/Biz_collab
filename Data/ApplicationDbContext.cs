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
        }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Client>()
                .HasKey(x => x.Id);
            builder.Entity<Group>()
                .HasKey(x => x.Id);
            builder.Entity<GroupClient>()
                .HasKey(x => new { x.ClientId, x.GroupId });
            builder.Entity<GroupClient>()
                .HasOne(x => x.Client)
                .WithMany(m => m.MyGroups)
                .HasForeignKey(x => x.ClientId);
            builder.Entity<GroupClient>()
                .HasOne(x => x.Group)
                .WithMany(m => m.Clients)
                .HasForeignKey(x => x.GroupId);
            builder.Entity<Transaction>();
            base.OnModelCreating(builder);
        }
    }
}
