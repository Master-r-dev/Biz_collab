using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz_collab.Models
{
    public class Vote
    {
        public string ClientId { get; set; }
        public string TransactionId { get; set; }
        public Client Client { get; set; }
        public Transaction Transaction { get; set; }        
        public bool V { get; set; } //Vote:  голос:да или нет       
        public int P { get; set; }  //Power: сила голоса 
    }
}
