using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Biz_collab.Models;
using Biz_collab.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace Biz_collab.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
            
        }
        
        [Authorize]
        public async Task<IActionResult> Index()
        {
            ClaimsPrincipal currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            var currentUserLog = currentUser.FindFirst(ClaimTypes.Name).Value;
            if ( _db.Clients.Find(currentUserID)==null )
            {
                Client client = new Client { Id = currentUserID, Login= currentUserLog, PersBudget=0 };
                _db.Clients.Add(client);
                _db.SaveChanges();
               return View(await _db.Clients.Include(c=>c.MyGroups).Where(c=>c.Id== currentUserID).ToListAsync());
            }
          
            return View();
        }
        public IActionResult About()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult AddBalance()
        {
            ClaimsPrincipal currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewBag.PersBudget = _db.Clients.Find(currentUserID).PersBudget;
            return View();
        }
     
        [HttpPost]
        public IActionResult  AddBalance(int Add)
        {
            if (Add <= 0)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ClaimsPrincipal currentUser = this.User;
                var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
                var client = _db.Clients.Find(currentUserID);
                client.PersBudget += Add;
                _db.Entry(client).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
