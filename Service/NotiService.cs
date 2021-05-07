using Biz_collab.IService;
using Biz_collab.Models;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Biz_collab.Data;

namespace Biz_collab.Service
{
    public class NotiService : INotiService
    {
        List<Notification> _oNotifications = new List<Notification>();
        List<MutedName> _MutedName = new List<MutedName>();
        private readonly ApplicationDbContext _db;
        public NotiService(ApplicationDbContext db)
        {
            _db = db;
        }
        public List<Notification> GetNotifications(string ClientId, bool bIsGetOnlyUnread)
        {
            _oNotifications = new List<Notification>();
            
                var oNotis = _db.Notifications.Where(n => n.ClientId == ClientId).ToList();
                if (oNotis != null && oNotis.Count() > 0)
                {
                    _oNotifications = oNotis;
                }
                return _oNotifications;
        }
        public List<MutedName> GetMutedNames(string ClientId)
        {
            _MutedName = new List<MutedName>();

            var oNames = _db.MutedNames.Where(n => n.ClientId == ClientId).ToList();
            if (oNames != null && oNames.Count() > 0)
            {
                _MutedName = oNames;
            }
            return _MutedName;
        }
    }
}
