using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Biz_collab.Models
{
    public class Client
    {
        public string Id = System.Web.HttpContext.Current.User.Identity.GetUserId();
        public string Login= System.Web.HttpContext.Current.User.Identity.GetUserName()
            .Substring(0, System.Web.HttpContext.Current.User.Identity.GetUserName().LastIndexOf("@") + 1); 
        public ICollection<Group> Groups { get; set; }

        public Client()
        {                     
               Groups = new List<Group>();            
        }
    }
}
