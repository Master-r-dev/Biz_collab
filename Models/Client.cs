using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Biz_collab.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Fname { get; set; }

        public ICollection<Group> Groups { get; set; }

        public Client()
        {                     
               Groups = new List<Group>();            
        }
    }
}