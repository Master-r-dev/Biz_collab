using System;
using System.ComponentModel.DataAnnotations;
namespace Biz_collab.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Text { get; set; }
        public DateTime Time { get; set; }
        public int RepliedTo { get; set; }
        public string ClientId { get; set; }
        public virtual Client Client { get; set; } //нужен для того чтобы редактировать/удалять свои сообщения
        public string GroupId { get; set; }
        public virtual Group Group { get; set; } //нужен чтобы не подгружать все сообщения,а только те которые принадлежат данной бд
        public Message() {
            Time = DateTime.Now;
        }
    }
}
