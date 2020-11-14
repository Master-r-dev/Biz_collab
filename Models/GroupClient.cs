using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz_collab.Models
{
    public class GroupClient
    {
        public string ClientId { get; set; }
        //public string ClientRole { get; set; }
        public Client Client { get; set; }
        public Group Group { get; set; }
        public string GroupId { get; set; }
    }
}
