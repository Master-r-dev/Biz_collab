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
        List<MutedList> _MutedList = new List<MutedList>();
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
        public List<MutedList> GetMutedList(string ClientId)
        {
            _MutedList = new List<MutedList>();

            var oMutedNames = _db.MutedLists.Where(n => n.ClientId == ClientId).ToList();
            if (oMutedNames != null && oMutedNames.Count() > 0)
            {
                _MutedList = oMutedNames;
            }
            return _MutedList;
        }
    }
}
