using Biz_collab.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz_collab.IService
{
    public interface INotiService
    {
        List<Notification> GetNotifications(string ClientId, bool bIsGetOnlyUnread);
    }
}
