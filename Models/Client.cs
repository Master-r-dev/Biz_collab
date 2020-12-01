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
using System.ComponentModel.DataAnnotations;

namespace Biz_collab.Models
{
    public class Client
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string Login { get; set; }
        public int PersBudget { get; set; } //поменять на int64 ?  на uint ?
        public ICollection<Role_Power> MyGroups { get; set; }// protected set ? 
        public ICollection<Transaction> MyTransactions { get; set; }
        public ICollection<Vote> MyVotes { get; set; }
        public Client()
        {                     
               MyGroups = new List<Role_Power>();
               MyTransactions = new List<Transaction>();
               MyVotes = new List<Vote>();
        }
        /*
          public string MakeLogin()
         {
            ClaimsPrincipal currentUser = this.User;
              string currentUserLog = currentUser.FindFirst(ClaimTypes.Name).Value
                 .Substring(0,currentUser.FindFirst(ClaimTypes.Name).Value.LastIndexOf("@") + 1);
        return currentUserLog
         }*/
    }
}
