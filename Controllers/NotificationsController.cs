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
        readonly INotiService _notiService = null;
        List<Notification> _oNotifications = new List<Notification>();
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
            switch (sortOrder)
            {
                case "notiheader_desc":
                    notifications = notifications.OrderByDescending(s => s.NotiHeader);                    
                    break;
                case "NotiHeader":
                    notifications = notifications.OrderBy(s => s.NotiHeader);                    
                    break;
                case "notibody_desc":
                    notifications = notifications.OrderByDescending(s => s.NotiBody);                   
                    break;
                case "NotiBody":
                    notifications = notifications.OrderBy(s => s.NotiBody);                   
                    break;
                case "IsRead":
                    notifications = notifications.OrderBy(s => s.IsRead);                    
                    break;
                case "isread_desc":
                    notifications = notifications.OrderByDescending(s => s.IsRead);                    
                    break;
                case "CreatedDate":
                    notifications = notifications.OrderBy(s => s.CreatedDate);
                    break;
                case "createddate_desc":
                    notifications = notifications.OrderByDescending(s => s.CreatedDate);
                    break;
                default:
                    notifications = notifications.OrderByDescending(s => s.CreatedDate);
                    break;
            }
            int pageSize = 5;
            return View(await PaginatedList<Notification>.CreateAsync(notifications, pageNumber ?? 1, pageSize));
        }

        public JsonResult GetNotifications(bool bIsGetOnlyUnread=false)
        {
            _oNotifications = new List<Notification>();
            _oNotifications = _notiService.GetNotifications(this.User.FindFirst(ClaimTypes.NameIdentifier).Value, bIsGetOnlyUnread);
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
            _db.SaveChanges();
            return RedirectToAction("/");
        }
        // GET: Transactions/Delete/5
        public IActionResult Delete(string id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var transaction = _db.Transactions
                 .Include(t => t.Client)
                 .Include(t => t.Votes)
                 .Include(t => t.Group)
                 .ThenInclude(g => g.Clients)
                 .ThenInclude(rp => rp.Client)
                 .FirstOrDefault(m => m.Id == id);
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;

            ViewBag.Name = transaction.Group.Name;
            if (transaction == null)
            {
                return NotFound();
            }
            ViewBag.Role = transaction.Group.Clients.First(rp => rp.ClientId == currentUserID).R;
            if (ViewBag.Role == "Забанен")
            {
                return RedirectToAction("BannedInGroup", new { name = transaction.Group.Name });
            }
            if (transaction.Status == true)
            {
                return RedirectToAction("OpenGroup", "Groups", new { name = transaction.Group.Name });
            }

            return PartialView(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var transaction = await _db.Transactions
                .Include(t => t.Client)
                .Include(t => t.Votes)
                .Include(t => t.Group)
                .ThenInclude(g => g.Clients)
                .ThenInclude(rp => rp.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (transaction.Group.Clients.FirstOrDefault(rp => rp.ClientId == currentUserID).R == "Забанен")
            {
                return RedirectToAction("BannedInGroup", new { name = transaction.Group.Name });
            }
            if (transaction.Status == true)
            {
                return RedirectToAction("OpenGroup", "Groups", new { name = transaction.Group.Name });
            }
            var name = transaction.Group.Name;
            _db.Votes.RemoveRange(transaction.Votes);
            _db.Transactions.Remove(transaction);
            await _db.SaveChangesAsync();
            return RedirectToAction("OpenGroup", "Groups", new { name = transaction.Group.Name });
        }
    }
}
