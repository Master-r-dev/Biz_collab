using Biz_collab.IService;
using Biz_collab.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz_collab.Controllers
{
    public class NotificationsController : Controller
    {
        INotiService _notiService = null;
        List<Noti> _oNotifications = new List<Noti>();
        public NotificationsController(INotiService notiService)
        {
            _notiService = notiService;
        }
        public IActionResult AllNotifications()
        {
            return View();
        }

        public JsonResult GetNotifications(bool bIsGetOnlyUnread=false)
        {
            int nToUserId = 3;
            _oNotifications = new List<Noti>();
            _oNotifications = _notiService.GetNotifications(nToUserId, bIsGetOnlyUnread);
            return Json(_oNotifications);
        }
    }
}
