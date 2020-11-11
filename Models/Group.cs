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
        public int Budget { get; set; } // стартовый бюджет группы
        public bool Type { get; set; } //с голосованием или нет
        //public int AmountClients { get; set; } кол-во участников
        //[maxAmount=AmountClients]
        public ICollection<GroupClient> Clients { get; set; }
        public Group()
        {
            Clients = new List<GroupClient>();
        }

    
        //public int EntryFee { get; set; }  минимальный внос для подключения в группу
        //public int MinPlus { get; set; } минимальное пополнение(защита от спама +1 у.е.)
        //public int MinMinus { get; set; }минимальный вычет(защита от спама -1 у.е.)
    }
}