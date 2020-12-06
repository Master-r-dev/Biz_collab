using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Biz_collab.Models
{
    public class Role_Power
    {
        [Required]
        public string ClientId { get; set; }
        [Required]
        public string R { get; set; } /*Роль(изменяемо модератором и создателем)
                                       * { Создатель - голос 100% также является модератором
                                            модератор-управляет всеми участниками кроме владельца его голос 40%) 
                                            важный участник(его голос= 5 участников)
                                            Участник-имеет голос и ставит на голосование(голос=1)
                                            Донатер-имеет голос но может только пополнять,не ставит на голосованее(голос= 1)
                                            Забанен-может только пополнять                                                                               
                                         }*/
        [Required]
        public int P { get; set; }/* Сила голоса по умолчанию {
                                                                   Роль="Забанен"
                                                                    Роль="Донатер" сила голоса=1 
                                                                    Роль="Участник" сила голоса=1
                                                                    Роль="VIP" сила голоса=25% от кол-ва пользователей
                                                                    Роль="Модератор" сила голоса=50% от кол-ва пользователей
                                                                }
                                   (опять же изменяемо модератором и создателем)*/
        public Client Client { get; set; }
        public Group Group { get; set; }
        [Required]
        public string GroupId { get; set; }
    }
}
