using Biz_collab.IService;
using Biz_collab.Models;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Biz_collab.Common;
using Dapper;

namespace Biz_collab.Service
{
    public class NotiService : INotiService
    {
        List<Noti> _oNotifications = new List<Noti>();
        public List<Noti> GetNotifications(string nToUserId, bool bIsGetOnlyUnread)
        {
            _oNotifications = new List<Noti>();
            using (IDbConnection con = new SqlConnection(Global.ConnectionString))
            {
                if (con.State == ConnectionState.Closed) con.Open();
                string conString = "SELECT * FROM Notification WHERE ToUserId=" + nToUserId;
                var oNotis = con.Query<Noti>(conString).ToList();

                if (oNotis != null && oNotis.Count() > 0)
                {
                    _oNotifications = oNotis;
                }
                return _oNotifications;
            }
        }
    }
}
