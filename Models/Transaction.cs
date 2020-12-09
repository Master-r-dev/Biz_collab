using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biz_collab.Models
{
    public class Transaction
    {
        [Key]
        public string Id { get; set; }
        public string ClientId { get; set; }
        public Client Client { get; set; }
        public string GroupId { get; set; }
        public Group Group { get; set; }
        [Required]
        public int Amount { get; set; }
        [Required]
        public byte OperationType { get; set; }  // Вычет или пополнение(между счетами группы и клиента). здесь должен быть enum     
        [Required]
        [StringLength(100)]
        public string Explanation { get; set; }
        public DateTime StartTime { get; set; }
        public bool Status { get; set; }//Выполнена или в ожидании
        public ICollection<Vote> Votes { get; set; }
        public float YesPercent { get; set; }
        public float NoPercent { get; set; }
        public Transaction()
        {
            Votes = new List<Vote> ();
        }

    }
}