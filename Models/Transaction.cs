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
        public string UserId { get; set; }
        [Required]
        public string GroupId { get; set; }
        [Required]
        public int Amount { get; set; }
        [Required]
        public bool OperationType { get; set; }  /// Вычет или пополнение
        [Required]
        public string Explanation { get; set; }
        [Required]
        public DateTime StartTime { get; set; }
      
    }
}