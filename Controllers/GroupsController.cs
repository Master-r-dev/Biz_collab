using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Biz_collab.Data;
using Biz_collab.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Biz_collab.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public GroupsController(ApplicationDbContext context)
        {
            _db = context;
        }

        // GET: Groups
        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(await _db.Groups.ToListAsync());
        }

        // GET: Groups/Details/5
        [Authorize]
        public async Task<IActionResult> Details(string id) //характеристики группы
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _db.Groups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@group == null)
            {
                return NotFound();
            }

            return View(@group);
        }
        [Authorize]

        public async Task<IActionResult> OpenGroup(string id,
            bool x, 
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber) //Сама группа
        {            
            if (id == null)
            {
                return NotFound();
            }
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var client = await _db.Clients.FirstAsync(c=>c.Id== currentUserID);
            
            var @group = await _db.Groups.Include(g=>g.Transactions).ThenInclude(t=>t.Votes).ThenInclude(v=>v.Client).Include(g => g.Clients).ThenInclude(rp=>rp.Client).FirstAsync(g => g.Id == id);            
            if (@group == null)
            {
                return NotFound();
            }
            if (@group.Clients.FirstOrDefault(rp=>rp.Client == client) ==null)
            {
                return RedirectToAction("JoinGroup",new { id=id });
            }
            var trans = _db.Transactions.Include(t=>t.Client).Include(t => t.Votes).ThenInclude(v => v.Client).Where(t => t.GroupId == id);
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
                trans = trans.Where(s => s.Client.Login.Contains(searchString) || s.Explanation.Contains(searchString));
            }
            ViewData["TimeSortParm"] = sortOrder == "Time" ? "time_desc" : "Time";
            ViewData["ClientNameSortParm"] = sortOrder == "Name" ? "name_desc" : "Name";
            ViewData["OperationTypeSortParm"] = sortOrder == "OperationType" ? "optype_desc" : "OperationType";
            ViewData["AmountSortParm"] = sortOrder == "Amount" ? "amount_desc" : "Amount";
            switch (sortOrder)
            {                
                case "OperationType":
                    trans = trans.OrderBy(s => s.OperationType);
                    break;                
                case "Time":
                    trans = trans.OrderBy(s => s.StartTime);
                    break;
                case "Name":
                    trans = trans.OrderBy(s => s.Client.Login);
                    break;
                case "Amount":
                    trans = trans.OrderBy(s => s.Amount);
                    break;
                case "amount_desc":
                    trans = trans.OrderByDescending(s => s.Amount);
                    break;
                case "time_desc":
                    trans = trans.OrderByDescending(s => s.StartTime);
                    break;
                case "optype_desc":
                    trans = trans.OrderByDescending(s => s.OperationType);
                    break;
                case "name_desc":
                    trans = trans.OrderByDescending(s => s.Client.Login);
                    break;
                default:
                    trans = trans.OrderByDescending(s => s.Status);
                    break;
            }
            int pageSize = 5;
            ViewBag.Transaction = await PaginatedList<Transaction>.CreateAsync(trans, pageNumber ?? 1, pageSize);
            if (x)
            {
                foreach (var i in @group.Transactions)
                {
                    int YesCounter = _db.Votes.Where(v => v.TransactionId == id && v.V == true).Sum(v => v.P);
                    int NoCounter = _db.Votes.Where(v => v.TransactionId == id && v.V == false).Sum(v => v.P);
                    i.YesPercent = ((float)YesCounter / @group.Clients.Count()) * 100.0f;
                    i.NoPercent = ((float)NoCounter / @group.Clients.Count()) * 100.0f;
                }
                foreach (var i in @group.Clients.Where(rp => rp.R == "VIP"))
                {
                    i.P = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(@group.Clients.Count() * 0.25)));
                    _db.Entry(i).State = EntityState.Modified;
                }
                foreach (var i in @group.Clients.Where(rp => rp.R == "Mod"))
                {
                    i.P = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(@group.Clients.Count() * 0.5)));
                    _db.Entry(i).State = EntityState.Modified;
                }
                @group.Clients.FirstOrDefault(rp =>rp.R == "Создатель").P = @group.Clients.Count();
                _db.Entry(group).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
            if (@group.Clients.FirstOrDefault(rp => rp.Client == client && rp.R == "Забанен") != null)
            {
                return RedirectToAction("BannedInGroup", new { id = id });
            }
            return View(@group);
        }
        [Authorize]
        public IActionResult BannedInGroup(string id)
        {
            ViewBag.groupName = _db.Groups.First(g => g.Id == id).Name;
            return View();
        }
        [Authorize]
        [HttpGet]
        public IActionResult JoinGroup(string id)
        {
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var client =  _db.Clients.First(c => c.Id == currentUserID);
            ViewBag.PersBudget =  client.PersBudget;
            var @group =  _db.Groups.Include(g => g.Clients).ThenInclude(rp => rp.Client).First(g => g.Id == id);
            if (group.EntryFeeDon!=-1) {
                if (client.PersBudget >= group.EntryFeeDon || client.PersBudget >= group.EntryFeeUser || client.PersBudget >= group.EntryFeeMod || client.PersBudget >= group.EntryFeeVIP) {
                    return View(@group);
                } 
            }
            return Redirect("~/Home/Index");
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> JoinGroup(string id,int sum)
        {
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var client = await _db.Clients.Include(c => c.MyGroups).FirstAsync(c => c.Id == currentUserID);
            var @group = await _db.Groups.Include(g => g.Clients).ThenInclude(rp => rp.Client).FirstAsync(g => g.Id == id);            
            if (@group.Clients.FirstOrDefault(rp => rp.Client == client) == null && group.Type == 1 && sum==1)
            {
                Role_Power cl = new Role_Power { Group = @group, Client = client, ClientId = client.Id, GroupId = @group.Id, R = "Донатер", P = 1 };
                @group.Clients.Add(cl);
                _db.Clients.FirstOrDefault(cr => cr.Id == currentUserID).MyGroups.Add(cl);
                client.PersBudget -= group.EntryFeeDon;
                group.Budget += group.EntryFeeDon;
                _db.Entry(client).State = EntityState.Modified;
                _db.Entry(group).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("OpenGroup", new { id = id ,x=true });
            }
            else if (@group.Clients.FirstOrDefault(rp => rp.Client == client) == null && group.Type == 2)
            {
                if (sum == 1 && client.PersBudget>=group.EntryFeeDon) {
                    Role_Power cl = new Role_Power { Group = @group, Client = client, ClientId = client.Id, GroupId = @group.Id, R = "Донатер", P = 1 };
                    @group.Clients.Add(cl);
                    _db.Clients.FirstOrDefault(cr => cr.Id == currentUserID).MyGroups.Add(cl);
                    client.PersBudget -= group.EntryFeeDon;
                    group.Budget += group.EntryFeeDon;
                    _db.Entry(client).State = EntityState.Modified;
                    _db.Entry(group).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    return RedirectToAction("OpenGroup", new { id = id,x=true });
                }
                if (sum == 2 && client.PersBudget >= group.EntryFeeUser) {
                    Role_Power cl = new Role_Power { Group = @group, Client = client, ClientId = client.Id, GroupId = @group.Id, R = "Участник", P = 1 }; 
                    @group.Clients.Add(cl);
                    _db.Clients.FirstOrDefault(cr => cr.Id == currentUserID).MyGroups.Add(cl);
                    client.PersBudget -= group.EntryFeeUser;
                    group.Budget += group.EntryFeeUser;
                    _db.Entry(client).State = EntityState.Modified;
                    _db.Entry(group).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    return RedirectToAction("OpenGroup", new { id = id,x=true });
                }
                if (sum == 3 && client.PersBudget >= group.EntryFeeVIP) {
                    Role_Power cl = new Role_Power { Group = @group, Client = client, ClientId = client.Id, GroupId = @group.Id, R = "VIP", P = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(_db.Groups.Find(id).Clients.Count() *0.25))) };
                    @group.Clients.Add(cl);
                    _db.Clients.FirstOrDefault(cr => cr.Id == currentUserID).MyGroups.Add(cl);
                    client.PersBudget -= group.EntryFeeVIP;
                    group.Budget += group.EntryFeeVIP;
                    _db.Entry(client).State = EntityState.Modified;
                    _db.Entry(group).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    return RedirectToAction("OpenGroup", new { id = id, x = true });
                }
                if (sum == 4 && client.PersBudget >= group.EntryFeeMod) {
                    Role_Power cl = new Role_Power { Group = @group, Client = client, ClientId = client.Id, GroupId = @group.Id, R = "Mod", P = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(_db.Groups.Find(id).Clients.Count() *0.5))) };
                    @group.Clients.Add(cl);
                    _db.Clients.FirstOrDefault(cr => cr.Id == currentUserID).MyGroups.Add(cl);
                    client.PersBudget -= group.EntryFeeMod;
                    group.Budget += group.EntryFeeMod;
                    _db.Entry(client).State = EntityState.Modified;
                    _db.Entry(group).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    return RedirectToAction("OpenGroup", new { id = id, x = true });
                }

            }
            return View(@group);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditRoleClient(string Login,string name,string role,int power)
        {   /*код изменение роли клиента*/
            var client = await _db.Role_Powers.Include(rp=>rp.Client).Include(rp=>rp.Group).FirstAsync(rp => rp.Client.Login == Login && rp.Group.Name==name);
            client.R = role;
            client.P = power;
            _db.Entry(client).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return View();
        }

       [Authorize]
        public async Task<IActionResult> BanClient(string Login, string id)
        {   /*код бан клиента*/
            var client = await _db.Role_Powers.Include(rp => rp.Client).FirstAsync(rp => rp.Client.Login == Login && rp.GroupId == id);
            client.R = "Забанен";
            client.P = 0;
            _db.Entry(client).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("OpenGroup", new { id = id });
        }
        [Authorize]
        // GET: Groups/Create
        public IActionResult Create()
        {
            ClaimsPrincipal currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewBag.PersBudget = Convert.ToInt32(_db.Clients.FirstOrDefault(cr => cr.Id == currentUserID).PersBudget);
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Group @group)
        {
            @group.Id = Guid.NewGuid().ToString();
            if (_db.Groups.Any(g => g.Name == group.Name && g.Id!=group.Id ) == true)
            {
                ModelState.AddModelError("Name", "Такое имя занято!");
            }
                if (ModelState.IsValid)
            {
                
                    //убедится что получает на вход текущего клиента
                    ClaimsPrincipal currentUser = this.User;
                    var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
                    //находим клиента который создает и добовляем его в группу ,даем роль создателя 
                    var client = _db.Clients.Include(c => c.MyGroups).FirstOrDefault(cr => cr.Id == currentUserID);
                    Role_Power cl = new Role_Power { Group = @group, Client = client, ClientId = client.Id, GroupId = @group.Id, R = "Создатель", P = 2 };
                    @group.Clients.Add(cl);
                    //у этого клиента вычитаем сумму которая выдалась на группу 
                    if (client.PersBudget >= @group.Budget) _db.Clients.FirstOrDefault(cr => cr.Id == currentUserID).PersBudget -= @group.Budget;
                    //добавляем созданую группу к создателю клиенту
                    _db.Clients.FirstOrDefault(cr => cr.Id == currentUserID).MyGroups.Add(cl);
                    _db.Entry(client).State = EntityState.Modified;
                    _db.Groups.Add(@group);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                
            }
            return View(@group);
        }

        // GET: Groups/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _db.Groups.FindAsync(id);
            if (@group == null)
            {
                return NotFound();
            }
            return View(@group);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Name,Type,CloseCall,EntryFeeDon,EntryFeeUser,EntryFeeVIP,EntryFeeMod,MinPlus,MinMinus")] Group @group)
        {
            if (id != @group.Id)
            {
                return NotFound();
            }
            if (_db.Groups.Any(g => g.Name == group.Name && g.Id != group.Id) == true)
            {
                ModelState.AddModelError("Name", "Такое имя занято!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(@group);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(@group.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(@group);
        }

        // GET: Groups/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _db.Groups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@group == null)
            {
                return NotFound();
            }

            return View(@group);
        }

        // POST: Groups/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var @group = await _db.Groups.FindAsync(id);
            _db.Groups.Remove(@group);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GroupExists(string id)
        {
            return _db.Groups.Any(e => e.Id == id);
        }
    }
}
