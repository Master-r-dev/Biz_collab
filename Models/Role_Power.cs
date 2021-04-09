using System.ComponentModel.DataAnnotations;

namespace Biz_collab.Models
{
    public class Role_Power
    {
        public Client Client { get; set; }
        public Group Group { get; set; }
        [Required]
        public string GroupId { get; set; }
        [Required]
        public string ClientId { get; set; }
        [Required]
        [StringLength(20)]
        public string R { get; set; } /*Роль(изменяемо модератором и создателем)
                                       * { Создатель - голос 100% также является модератором
                                            модератор-управляет всеми участниками кроме владельца его голос 50%) 
                                            важный участник-имеет голос и ставит на голосование(его голос= 1/4 от кол-ва участников)
                                            Участник-имеет голос и ставит на голосование(голос=1)
                                            Донатер-имеет голос но может только пополнять,не ставит на голосованее(голос= 1)
                                            Забанен                                                                             
                                         }*/
        [Required]
        public int P { get; set; }/* Сила голоса (изменяемо)*/
        public double? Percent { get; set; }       
    }
}
