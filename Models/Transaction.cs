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
        [Required]
        public string ClientId { get; set; }
        [Required]
        public string ClientName { get; set; }
        [Required]
        public string GroupId { get; set; }
        [Required]
        public int Amount { get; set; }//поменять на int64 ?
        [Required]
        public short OperationType { get; set; }  /// Вычет или пополнение(между счетами груупы и клиента)
        /* public enum OperationType : byte { add=1, exch=2, sub=3 }*/
        [Required]
        public string Explanation { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
      
    }
}