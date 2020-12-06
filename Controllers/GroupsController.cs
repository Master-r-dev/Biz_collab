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

        public async Task<IActionResult> OpenGroup(string id) //Сама группа
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
            if (@group.Clients.FirstOrDefault(rp => rp.Client == client && rp.R=="VIP") != null)
            {
                @group.Clients.FirstOrDefault(rp => rp.Client == client && rp.R == "VIP").P= Convert.ToInt32(Math.Ceiling(Convert.ToDouble(_db.Groups.Find(id).Clients.Count() * 0.25)));
                _db.Entry(group).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
            else if (@group.Clients.FirstOrDefault(rp => rp.Client == client && rp.R == "Mod") != null)
            {
                @group.Clients.FirstOrDefault(rp => rp.Client == client && rp.R == "Mod").P = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(_db.Groups.Find(id).Clients.Count() * 0.5)));
                _db.Entry(group).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
            else if (@group.Clients.FirstOrDefault(rp => rp.Client == client && rp.R == "Забанен") != null)
            {
                return RedirectToAction("BannedInGroup", new { id = id });
            }
            if (@group.Clients.FirstOrDefault(rp => rp.Client == client && rp.R == "Создатель") != null)
            {
                @group.Clients.FirstOrDefault(rp => rp.Client == client && rp.R == "Создатель").P = _db.Groups.Find(id).Clients.Count();
                _db.Entry(group).State = EntityState.Modified;
                await _db.SaveChangesAsync();
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
            if (client.PersBudget>=group.EntryFeeDon) {
                return View(@group);
            }
            else { return RedirectToAction(nameof(Index)); }
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
                return RedirectToAction("OpenGroup", new { id = id });
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
                    return RedirectToAction("OpenGroup", new { id = id });
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
                    return RedirectToAction("OpenGroup", new { id = id });
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
                    return RedirectToAction("OpenGroup", new { id = id });
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
                    return RedirectToAction("OpenGroup", new { id = id });
                }

            }
            return View(@group);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditRoleClient(string Login,string id,string role,int power)
        {   /*код изменение роли клиента*/
            var client = await _db.Role_Powers.Include(rp=>rp.Client).FirstAsync(rp => rp.Client.Login == Login && rp.GroupId==id);
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
            if (ModelState.IsValid)
            {
                //убедится что получает на вход текущего клиента
                ClaimsPrincipal currentUser = this.User;
                var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
                //находим клиента который создает и добовляем его в группу ,даем роль создателя 
                var client = _db.Clients.Include(c => c.MyGroups).FirstOrDefault(cr => cr.Id == currentUserID);
                Role_Power cl = new Role_Power { Group = @group, Client = client, ClientId = client.Id, GroupId = @group.Id, R = "Создатель", P = _db.Groups.Find(@group.Id).Clients.Count() };//overkill value ;)
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
        public async Task<IActionResult> Edit(string id, [Bind("Name,Budget,Type,CloseCall,EntryFeeDon,EntryFeeUser,EntryFeeVIP,EntryFeeMod,MinPlus,MinMinus")] Group @group)
        {
            if (id != @group.Id)
            {
                return NotFound();
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
