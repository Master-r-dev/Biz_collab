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
        public string Id { get; set; }
        private IGroup provider;
        public string Login { get; set; }
        public int PersBudget { get; set; }
        public ICollection<Group> MyGroups { get; set; }// protected set ?

        public Client()
        {                     
               MyGroups = new List<Group>();            
        }
        public void SetGroup(string groupName, int Budget, bool Type)
        {
            MyGroups = (ICollection<Group>)provider.GetOrAddGroup(groupName, Budget, Type);
        }
        public Client(IGroup provider)
        {
            this.provider = provider;
        }
        /*   private readonly IHttpContextAccessor _httpContextAccessor;
        public Client(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        
        }

        public void GetIdofUser()
        {
            string Id = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
           
        }
        public void GetLoginofUser()
        {
            string Login = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email)
                .Substring(0, _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email).LastIndexOf("@") + 1);

        }*/
    }
}
