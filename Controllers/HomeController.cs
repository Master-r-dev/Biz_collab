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
        public async Task<IActionResult> Index(
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber)
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
            var mycreatedgroups = _db.Groups.Include(g => g.Clients).ThenInclude(rp => rp.Client).Where(g => g.Clients.First(rp => rp.ClientId == currentUserID && rp.R == "Создатель") != null);
            var mygroups= _db.Groups.Include(g => g.Clients).ThenInclude(rp => rp.Client).Where(g => g.Clients.First(rp => rp.ClientId == currentUserID && rp.R!="Забанен" && rp.R != "Создатель") != null);            
            var AllGroups = _db.Groups.Include(g => g.Clients).ThenInclude(rp=>rp.Client).AsQueryable();
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
                AllGroups = AllGroups.Where(s => s.Name.Contains(searchString));
                mygroups = mygroups.Where(s => s.Name.Contains(searchString));
                mycreatedgroups= mycreatedgroups.Where(s => s.Name.Contains(searchString));
            }
            ViewData["ClientAmountSortParm"] = sortOrder == "ClientAmount" ? "clientamount_desc" : "ClientAmount";
            ViewData["NameSortParm"] = sortOrder == "Name" ? "name_desc" : "Name";
            ViewData["BudgetSortParm"] = sortOrder == "Budget" ? "budget_desc" : "Budget";            
            switch (sortOrder)
            {
                case "name_desc":
                    AllGroups = AllGroups.OrderByDescending(s => s.Name);
                    mygroups = mygroups.OrderByDescending(s => s.Name);
                    mycreatedgroups = mycreatedgroups.OrderByDescending(s => s.Name);
                    break;
                case "Budget":
                    AllGroups = AllGroups.OrderBy(s => s.Budget);
                    mygroups = mygroups.OrderBy(s => s.Budget);
                    mycreatedgroups = mycreatedgroups.OrderBy(s => s.Budget);
                    break;
                case "budget_desc":
                    AllGroups = AllGroups.OrderByDescending(s => s.Budget);
                    mygroups = mygroups.OrderByDescending(s => s.Budget);
                    mycreatedgroups = mycreatedgroups.OrderByDescending(s => s.Budget);
                    break;
                case "ClientAmount":
                    AllGroups = AllGroups.OrderBy(s => s.Clients.Count);
                    mygroups = mygroups.OrderBy(s => s.Clients.Count);
                    mycreatedgroups = mycreatedgroups.OrderBy(s => s.Clients.Count);
                    break;
                case "Name":
                    AllGroups = AllGroups.OrderBy(s => s.Name);
                    mygroups = mygroups.OrderBy(s => s.Name);
                    mycreatedgroups = mycreatedgroups.OrderBy(s => s.Name);
                    break;
                case "clientamount_desc":                    
                    AllGroups = AllGroups.OrderByDescending(s => s.Clients.Count);
                    mygroups = mygroups.OrderByDescending(s => s.Clients.Count);
                    mycreatedgroups = mycreatedgroups.OrderByDescending(s => s.Clients.Count);
                    break;
                default:
                    AllGroups = AllGroups.OrderByDescending(s => s.Clients.Count);
                    mygroups = mygroups.OrderByDescending(s => s.Clients.Count);
                    mycreatedgroups = mycreatedgroups.OrderByDescending(s => s.Clients.Count);
                    break;
            }
            int pageSize = 3;
            int pageSize2 = 2;
            ViewBag.MyGroups = await PaginatedList<Group>.CreateAsync(mygroups, pageNumber ?? 1, pageSize2);
            ViewBag.MyCreatedGroups = await PaginatedList<Group>.CreateAsync(mycreatedgroups, pageNumber ?? 1, pageSize2);
            ViewBag.PersBudget = _db.Clients.First(c=>c.Id==currentUserID).PersBudget;
            ViewBag.AddBalanceTitle = "Пополнить баланс: " + Convert.ToString(ViewBag.PersBudget);
            return View(await PaginatedList<Group>.CreateAsync(AllGroups, pageNumber ?? 1, pageSize));
        }
        public IActionResult About()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult AddBalance()
        {
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewBag.PersBudget = _db.Clients.Find(currentUserID).PersBudget;
            return PartialView();
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
                var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
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
