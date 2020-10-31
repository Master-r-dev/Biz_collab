using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz_collab.Models
{
    public interface IGroup
    {
        Group GetOrAddGroup(string groupName, int Budget, bool Type);
    }
}
