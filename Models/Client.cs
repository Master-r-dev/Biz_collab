using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
namespace Biz_collab.Models
{
    public class Client
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        public Client(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        
        }

        public void GetIdofUser()
        {
            string Id = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
           
        }
        public string Id { get; set; }

        public string Login { get; set; }
        public ICollection<Group> Groups { get; set; }

        public Client()
        {                     
               Groups = new List<Group>();            
        }
    }
}