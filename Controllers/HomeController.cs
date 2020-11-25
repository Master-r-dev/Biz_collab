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
        public async Task<IActionResult> Index(string sortOrder)
        {
            ClaimsPrincipal currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            var currentUserLog = currentUser.FindFirst(ClaimTypes.Name).Value;
            if ( _db.Clients.Find(currentUserID)==null )
            {
                Client client = new Client { Id = currentUserID, Login= currentUserLog, PersBudget=0 };
                _db.Clients.Add(client);
                _db.SaveChanges();
            }
            var mygroups= _db.Groups.Include(g => g.Clients).Where(g => g.Clients.First(rp => rp.ClientId == currentUserID) != null);            
            var AllGroups = from s in _db.Groups
                            select s;
            ViewData["ClientAmountSortParm"] = String.IsNullOrEmpty(sortOrder) ? "" : "ClientAmount";
            ViewData["NameSortParm"] = sortOrder == "Name" ? "name_desc" : "";
            ViewData["BudgetSortParm"] = sortOrder == "Budget" ? "budget_desc" : "Budget";            
            switch (sortOrder)
            {
                case "name_desc":
                    AllGroups = AllGroups.OrderByDescending(s => s.Name);
                    mygroups = mygroups.OrderByDescending(s => s.Name);
                    break;
                case "Budget":
                    AllGroups = AllGroups.OrderBy(s => s.Budget);
                    mygroups = mygroups.OrderBy(s => s.Budget);
                    break;
                case "budget_desc":
                    AllGroups = AllGroups.OrderByDescending(s => s.Budget);
                    mygroups = mygroups.OrderByDescending(s => s.Budget);
                    break;
                case "ClientAmount":
                    AllGroups = AllGroups.OrderBy(s => s.Clients.Count);
                    mygroups = mygroups.OrderBy(s => s.Clients.Count);
                    break;
                case "Name":
                    AllGroups = AllGroups.OrderBy(s => s.Name);
                    mygroups = mygroups.OrderBy(s => s.Name);
                    break;
                default:                    
                    AllGroups = AllGroups.OrderByDescending(s => s.Clients.Count);
                    mygroups = mygroups.OrderByDescending(s => s.Clients.Count);
                    break;
            }
            ViewBag.MyGroups = mygroups.ToList();
            return View(await AllGroups.ToListAsync());
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
