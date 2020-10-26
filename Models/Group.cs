using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Biz_collab.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Budget { get; set; }
        public ICollection<Client> Clients { get; set; }
        public Group()
        {
            Clients = new List<Client>();
        }
    }
}