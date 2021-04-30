using Biz_collab.Data;
using Biz_collab.IService;
using Biz_collab.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Biz_collab.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _db;
        INotiService _notiService = null;
        List<Notification> _oNotifications = new List<Notification>();
        public NotificationsController(INotiService notiService, ApplicationDbContext context)
        {
            _notiService = notiService;
            _db = context;
        }
        public IActionResult AllNotifications()
        {
            return View();
        }

        public JsonResult GetNotifications(bool bIsGetOnlyUnread=false)
        {
            string nToUserId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            _oNotifications = new List<Notification>();
            _oNotifications = _notiService.GetNotifications(nToUserId, bIsGetOnlyUnread);
            return Json(_oNotifications);
        }
        [Route("/notificationSeen")]
        [HttpPut]
        public ActionResult NotificationSeen()
        {
            var bodyStr = "";
            var req = Request;
            using (StreamReader reader
                  = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
            {
                bodyStr = reader.ReadToEndAsync().Result;
            }
            var json = JsonConvert.DeserializeObject<Notification>(bodyStr);
            _db.Entry(json).State = EntityState.Modified;
            Console.WriteLine(json.IsRead);
            _db.SaveChanges();
            return RedirectToAction("/");
        }
    }
}
