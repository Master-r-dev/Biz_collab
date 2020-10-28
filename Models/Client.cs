using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Biz_collab.Models
{
    public class Client
    {
        public string Id = System.Web.HttpContext.Current.User.Identity.GetUserId();
        public string Login { get; set; }
        public ICollection<Group> Groups { get; set; }

        public Client()
        {                     
               Groups = new List<Group>();            
        }
    }
}
