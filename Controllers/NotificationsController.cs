using Biz_collab.IService;
using Biz_collab.Models;
using Microsoft.AspNetCore.Mvc;
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
        INotiService _notiService = null;
        List<Notification> _oNotifications = new List<Notification>();
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
            string nToUserId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            _oNotifications = new List<Notification>();
            _oNotifications = _notiService.GetNotifications(nToUserId, bIsGetOnlyUnread);
            return Json(_oNotifications);
        }
        [Route("/notificationSeen")]
        [HttpPut]
        public ActionResult Cart()
        {
            var bodyStr = "";
            var req = Request;
            using (StreamReader reader
                  = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
            {
                bodyStr = reader.ReadToEndAsync().Result;
            }
            var json = JsonConvert.DeserializeObject<Notification>(bodyStr);
            //СЮДА ЛЕХА СЮДА ЛЕХА СЮДА ЛЕХА СЮДА ЛЕХА СЮДА ЛЕХА СЮДА ЛЕХА СЮДА ЛЕХА СЮДА ЛЕХА СЮДА ЛЕХА СЮДА ЛЕХА СЮДА ЛЕХА СЮДА ЛЕХА СЮДА ЛЕХА СЮДА ЛЕХА СЮДА
            //ниже как было реализовано в старом проекте(работало)
            /*Thread.Sleep(500);
            var totalPrice = 0;
            int lastCustomerId = db.Customers.Max(item => item.customerId);
            foreach (var value in json.Dat)
            {
                totalPrice += value.Price * value.Quantity;
            }
            db.Orders.Add(new order { order_time = DateTime.Now, price = totalPrice, customerId = lastCustomerId });
            db.SaveChanges();
            int lastOrderId = db.Orders.Max(item => item.orderId);
            foreach (var value in json.Dat)
            {
                db.Mos.Add(new mo { orderId = lastOrderId, menu_itemId = value.Id, order_items_quantity = value.Quantity });
            }
            db.SaveChanges();*/
            return RedirectToAction("/");
        }
    }
}
