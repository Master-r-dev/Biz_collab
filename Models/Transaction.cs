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
        public int Id { get; set; }        
        public string ClientId { get; set; }
        public Client Client { get; set; }       
        public string GroupId { get; set; }
        public Group Group { get; set; }
        [Required]
        public int Amount { get; set; }///         поменять на int64 ? на uint ?
        [Required]
        public byte OperationType { get; set; }  // Вычет или пополнение(между счетами группы и клиента)        
        [Required]
        public string Explanation { get; set; }
        public DateTime StartTime { get; set; }
        public bool Status { get; set; }//Выполнена или в ожидании
        public ICollection<Vote> Votes { get; set; }
        public double YesPercent { get; set; }
        public double NoPercent { get; set; }
        public Transaction()
        {
            Votes = new List<Vote> {};
        }

    }
}