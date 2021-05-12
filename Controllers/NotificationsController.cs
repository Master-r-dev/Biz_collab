using Biz_collab.Data;
using Biz_collab.IService;
using Biz_collab.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Biz_collab.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _db;
        readonly INotiService _notiService = null;
        List<Notification> _oNotifications = new List<Notification>();
        List<MutedName> _MutedName = new List<MutedName>();
        public NotificationsController(INotiService notiService, ApplicationDbContext context)
        {
            _notiService = notiService;
            _db = context;
        }
        public async Task<IActionResult> AllNotifications(string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber)
        {            
            var notifications = _db.Notifications.Where(n => n.ClientId == this.User.FindFirst(ClaimTypes.NameIdentifier).Value).AsQueryable();   
            ViewData["CurrentSort"] = sortOrder;
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;
            if (!String.IsNullOrEmpty(searchString))
            {
                notifications = notifications.Where(s => s.NotiHeader.Contains(searchString) || s.NotiBody.Contains(searchString));
            }
            ViewData["NotiHeaderSortParm"] = sortOrder == "NotiHeader" ? "notiheader_desc" : "NotiHeader";
            ViewData["NotiBodySortParm"] = sortOrder == "NotiBody" ? "notibody_desc" : "NotiBody";
            ViewData["IsReadSortParm"] = sortOrder == "IsRead" ? "isread_desc" : "IsRead";
            ViewData["CreatedDateSortParm"] = sortOrder == "CreatedDate" ? "createddate_desc" : "CreatedDate";
            notifications = sortOrder switch
            {
                "notiheader_desc" => notifications.OrderByDescending(s => s.NotiHeader),
                "NotiHeader" => notifications.OrderBy(s => s.NotiHeader),
                "notibody_desc" => notifications.OrderByDescending(s => s.NotiBody),
                "NotiBody" => notifications.OrderBy(s => s.NotiBody),
                "IsRead" => notifications.OrderBy(s => s.IsRead),
                "isread_desc" => notifications.OrderByDescending(s => s.IsRead),
                "CreatedDate" => notifications.OrderBy(s => s.CreatedDate),
                "createddate_desc" => notifications.OrderByDescending(s => s.CreatedDate),
                _ => notifications.OrderByDescending(s => s.CreatedDate),
            };
            int pageSize = 5;
            ViewBag.MutedName = _db.MutedNames.Where(n => n.ClientId == this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            return View(await PaginatedList<Notification>.CreateAsync(notifications, pageNumber ?? 1, pageSize));
        }

        public JsonResult GetNotifications(bool bIsGetOnlyUnread=false)
        {
            _oNotifications = new List<Notification>();
            _oNotifications = _notiService.GetNotifications(this.User.FindFirst(ClaimTypes.NameIdentifier).Value, bIsGetOnlyUnread);
            return Json(_oNotifications);
        }

        public JsonResult GetMutedNames()
        {
            _MutedName = new List<MutedName>();
            _MutedName = _notiService.GetMutedNames(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            return Json(_MutedName);
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
            if (_db.Notifications.Any(n => n.Id == json.Id && n.IsRead==false))
            {
                _db.Entry(json).State = EntityState.Modified;
                _db.SaveChanges();
            }
            return Redirect(Request.Headers["Referer"].ToString());
        }
        public IActionResult Accept(int? id,string name)
        {
            if (id == null)
            {
                return NotFound();
            } 
            var n =_db.Notifications.FirstOrDefault(m => m.Id == id);
            Regex regex = new Regex(@"(?<=[:])[^\s]+");
            var matches = regex.Matches(n.NotiHeader);
            if (n.NotiHeader.IndexOf("про") == -1) //пользователь без процента
            {
                Role_Power cl = new Role_Power
                {
                    ClientId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value,
                    GroupId = _db.Groups.First(g=>g.Name==name).Id,
                    R = matches[0].Value,
                    P = int.Parse(matches[1].Value, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite) 
                };
                _db.Role_Powers.Add(cl);                
            }
            else //с процентом
            {
                Role_Power cl = new Role_Power
                {
                    ClientId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value,
                    GroupId = _db.Groups.First(g => g.Name == name).Id,
                    R = matches[0].Value,
                    P = int.Parse(matches[1].Value, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite),
                    Percent = Math.Round(Convert.ToDouble(matches[2].Value, CultureInfo.InvariantCulture) * .0001, 4)
            };
                _db.Role_Powers.Add(cl);
            }
            n.NotiHeader= "ПРИНЯТО " + n.NotiHeader;
            n.Url = "../Groups/OpenGroup?name=" + name;
            _db.Entry(n).State = EntityState.Modified;
            _db.SaveChanges();
            return Redirect(Request.Headers["Referer"].ToString());
        }
        public IActionResult Mute( string name)
        {
            if (name == null)
            {
                return NotFound();
            }
            if (_db.MutedNames.Any(m => m.ClientId== this.User.FindFirst(ClaimTypes.NameIdentifier).Value && m.Name == name))
            {
                var n = _db.MutedNames.FirstOrDefault(m => m.ClientId == this.User.FindFirst(ClaimTypes.NameIdentifier).Value && m.Name == name);
                _db.MutedNames.Remove(n);
            }
            else {
                MutedName n = new MutedName
                {
                    ClientId = this.User.FindFirst(ClaimTypes.NameIdentifier).Value,
                    Name = name
                };
                _db.MutedNames.Add(n);
            }            
            _db.SaveChanges();
            return Redirect(Request.Headers["Referer"].ToString());
        }
        
        public async Task<IActionResult> Delete(int? id, bool act , string login, string name)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (act && !_db.MutedNames.Any(m => m.ClientId == _db.Clients.FirstOrDefault(c => c.Login == login).Id && m.Name == this.User.FindFirst(ClaimTypes.Name).Value)) {// удалить и отказаться от приглашения(отослать уведомление об отказе тому кто отправил приглашение)
                
                Notification declineOffer = new Notification
                {
                    ClientId =  _db.Clients.FirstOrDefault(c=>c.Login==login).Id,
                    NotiHeader = "Отказ вашему приглашению",
                    NotiBody = " Пользователем-" + this.User.FindFirst(ClaimTypes.Name).Value + " в группу:" + name,
                    IsRead = false,
                    Url = "../Groups/OpenGroup?name=" + name
                };
                await _db.Notifications.AddAsync(declineOffer);
                var n = _db.Notifications.Find(id);
                _db.Notifications.Remove(n);
            }
            else {//просто удалить уведомление  
                var n = _db.Notifications.Find(id);
                _db.Notifications.Remove(n);
            }    
            await _db.SaveChangesAsync();
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
