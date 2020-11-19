using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Biz_collab.Models
{
    public class Group
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public int Budget { get; set; } //  бюджет группы : поменять на int64 ? на uint ?
        public byte Type { get; set; } //min=0 for userinput (do in a view of radioboxes!!)
        public ICollection<Role_Power> Clients { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
        public Group()
        {
            Clients = new List<Role_Power>();
            Transactions = new List<Transaction>();
        }
        /*public struct Settings {*/
        public bool CloseCall { get; set; } //Что делать если голоса за и против равны?(1-принять,0-удалить)
        public int EntryFeeDon { get; set; } // минимальный внос для подключения в группу /Роль="Донатер" сила голоса=1      
        public int EntryFeeUser { get; set; } // минимальный внос для подключения в группу /Роль="Участник" сила голоса=1
        public int EntryFeeVIP { get; set; } // минимальный внос для подключения в группу /Роль="VIP" сила голоса=25% от кол-ва пользователей
        public int EntryFeeMod { get; set; } // минимальный внос для подключения в группу /Роль="Модератор" сила голоса=50% от кол-ва пользователей
        public int MinPlus { get; set; } //минимальное пополнение(защита от спама +1 у.е.)
        public int MinMinus { get; set; } //минимальный вычет(защита от спама -1 у.е.)
       /* }*/

      
    }
}