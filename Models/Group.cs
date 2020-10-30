using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Biz_collab.Models
{
    public class Group
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Budget { get; set; }
        public bool Type { get; set; } //с голосованяием или нет
        //public int AmountClients { get; set; }
        //[maxAmount=AmountClients]
        public ICollection<Client> Clients { get; set; }
        public Group()
        {
            Clients = new List<Client>();
        }
        //public int MinPlus { get; set; }
        //public int MinMinus { get; set; }
    }
}