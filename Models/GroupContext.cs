using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace Biz_collab.Models
{
    public class GroupContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

    }
}
