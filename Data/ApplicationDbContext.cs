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
            builder.Entity<Client>();
            builder.Entity<Group>();
            builder.Entity<Transaction>();
            base.OnModelCreating(builder);

          /*  builder.Entity<Client>()
                .HasMany(b => b.IncomingTransfers)
                .WithOne(t => t.Destination);

            builder.Entity<Client>()
                .HasMany(b => b.OutgoingTransfers)
                .WithOne(t => t.Origin);*/
        }
    }
}
