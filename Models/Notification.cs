using System;

namespace Biz_collab.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public Client Client { get; set; }
        public string ClientId { get; set; }
        public string NotiHeader { get; set; } = "";
        public string NotiBody { get; set; } = "";
        public bool IsRead { get; set; } = false;
        public string Url { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;        
    }
}
