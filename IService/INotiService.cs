using Biz_collab.Models;
using System.Collections.Generic;

namespace Biz_collab.IService
{
    public interface INotiService
    {
        List<Notification> GetNotifications(string ClientId, bool bIsGetOnlyUnread);
    }
}
