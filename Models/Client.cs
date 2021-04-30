using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Biz_collab.Models
{
    public class Client 
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string Login { get; set; }
        public int PersBudget { get; set; } 
        public ICollection<Role_Power> MyGroups { get; set; }
        public ICollection<Transaction> MyTransactions { get; set; }
        public ICollection<Notification>  MyNotifications { get; set; }
    public ICollection<Vote> MyVotes { get; set; }
        //для изменения и удаления
        public virtual ICollection<Message> MyMessages { get; set; }
        public Client()
        {               
               MyMessages = new HashSet<Message>();
               MyGroups = new List<Role_Power>();
               MyTransactions = new List<Transaction>();
               MyVotes = new List<Vote>();
        }        
    }
}
