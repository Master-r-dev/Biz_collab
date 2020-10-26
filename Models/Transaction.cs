using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lab1.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Amount { get; set; }
       
        public int OperationType { get; set; }  /// Вычет или пополнение
        public string Explanation { get; set; }
        public DateTime StartTime { get; set; }
      
    }
}