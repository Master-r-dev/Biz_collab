using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz_collab.Models
{
    public class MutedList
    {
        public string ClientId { get; set; }
        public virtual Client Client { get; set; }
        public string MutedId { get; set; }
    }
}
