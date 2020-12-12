﻿using System;
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
using System.Globalization;

namespace Biz_collab.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public GroupsController(ApplicationDbContext context)
        {
            _db = context;
        }
        // GET: Groups/Details/5
        [Authorize]
        public IActionResult Details(string name) //характеристики группы
        {
            if (name == null)
            {
                return NotFound();
            }

            var group = _db.Groups.AsNoTracking().FirstOrDefault(m => m.Name == name);
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var client = _db.Clients.AsNoTracking().First(c => c.Id == currentUserID);
            if (client.PersBudget >= group.EntryFeeDon || client.PersBudget >= group.EntryFeeUser || client.PersBudget >= group.EntryFeeMod || client.PersBudget >= group.EntryFeeVIP)
                ViewBag.Pass = true;
            else
                ViewBag.Pass = false;

            if (group != null)
            {
                return PartialView(group);
            }
            return NotFound();
        }
        [Authorize]

        public async Task<IActionResult> OpenGroup(string name,
            bool x, 
            string sortOrder,
            string currentFilter,
            string searchString,
            int? pageNumber) //Сама группа
        {            
            if (name == null)
            {
                return NotFound();
            }
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var client = await _db.Clients.AsNoTracking().FirstAsync(c=>c.Id== currentUserID);            
            var @group = await _db.Groups.Include(g => g.Clients).ThenInclude(rp=>rp.Client).FirstAsync(g => g.Name == name);
            if (@group.Clients.FirstOrDefault(rp => rp.Client == client && rp.R == "Забанен") != null)
            {
                return RedirectToAction("BannedInGroup", new { name });
            }
            if (@group == null)
            {
                return NotFound();
            }
            if (@group.Clients.FirstOrDefault(rp=>rp.ClientId == client.Id) ==null)
            {
                return RedirectToAction("JoinGroup",new { name });
            }
            var trans = _db.Transactions.Include(t=>t.Client).ThenInclude(c=>c.MyGroups).ThenInclude(rp=>rp.Group).Include(t => t.Votes).ThenInclude(v => v.Client).Where(t => t.GroupId == @group.Id);
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
                trans = trans.Where(s => s.Client.Login.Contains(searchString) || s.Client.MyGroups.First(rp=>rp.GroupId==s.GroupId && rp.R==searchString)!=null || s.Explanation.Contains(searchString));
            }
            ViewData["TimeSortParm"] = sortOrder == "Time" ? "time_desc" : "Time";
            ViewData["ClientNameSortParm"] = sortOrder == "Name" ? "name_desc" : "Name";
            ViewData["OperationTypeSortParm"] = sortOrder == "OperationType" ? "optype_desc" : "OperationType";
            ViewData["AmountSortParm"] = sortOrder == "Amount" ? "amount_desc" : "Amount";
            trans = sortOrder switch
            {
                "OperationType" => trans.OrderBy(s => s.OperationType),
                "Time" => trans.OrderBy(s => s.StartTime),
                "Name" => trans.OrderBy(s => s.Client.Login),
                "Amount" => trans.OrderBy(s => s.Amount),
                "amount_desc" => trans.OrderByDescending(s => s.Amount),
                "time_desc" => trans.OrderByDescending(s => s.StartTime),
                "optype_desc" => trans.OrderByDescending(s => s.OperationType),
                "name_desc" => trans.OrderByDescending(s => s.Client.Login),
                _ => trans.OrderByDescending(s => s.Status),
            };
            int pageSize = 5;
            if (x)
            {
                foreach (var i in trans)
                {
                    if (i.Status==false) {
                        int YesCounter = _db.Votes.Where(v => v.TransactionId == i.Id && v.V == true).Sum(v => v.P);
                        int NoCounter = _db.Votes.Where(v => v.TransactionId == i.Id && v.V == false).Sum(v => v.P);
                        i.YesPercent = ((float)YesCounter / @group.Clients.Count()) * 100.0f;
                        i.NoPercent = ((float)NoCounter / @group.Clients.Count()) * 100.0f;
                        _db.Entry(i).State = EntityState.Modified;
                    }
                }
                foreach (var i in @group.Clients.Where(rp => rp.R == "VIP"))
                {
                    i.P = Convert.ToInt32(Math.Round(Convert.ToDouble(@group.Clients.Count() * 0.25)));
                    _db.Entry(i).State = EntityState.Modified;
                }
                foreach (var i in @group.Clients.Where(rp => rp.R == "Mod"))
                {
                    i.P = Convert.ToInt32(Math.Floor(Convert.ToDouble(@group.Clients.Count() * 0.5)));
                    _db.Entry(i).State = EntityState.Modified;
                }
                foreach (var i in @group.Clients.Where(rp => rp.Percent!=null))
                {
                    i.P = Convert.ToInt32(Math.Round(Convert.ToDouble(@group.Clients.Count() * i.Percent)));
                    _db.Entry(i).State = EntityState.Modified;
                }
                @group.Clients.FirstOrDefault(rp =>rp.R == "Создатель").P = @group.Clients.Count();
                _db.Entry(group).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
             ViewBag.Transactions = await PaginatedList<Transaction>.CreateAsync(trans, pageNumber ?? 1, pageSize);            
             return View(@group);
        }
        [Authorize]
        public IActionResult BannedInGroup(string name)
        {
            ViewBag.groupName = _db.Groups.AsNoTracking().First(g => g.Name == name).Name;
            return View();
        }
        [Authorize]
        [HttpGet]
        public IActionResult JoinGroup(string name)
        {
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var client =  _db.Clients.AsNoTracking().First(c => c.Id == currentUserID);
            ViewBag.PersBudget =  client.PersBudget;
            var @group =  _db.Groups.AsNoTracking().First(g => g.Name == name);
            if (group.EntryFeeDon!=-1) {
                if (client.PersBudget >= group.EntryFeeDon || client.PersBudget >= group.EntryFeeUser || client.PersBudget >= group.EntryFeeMod || client.PersBudget >= group.EntryFeeVIP) {
                    return View(@group);
                } 
            }
            return Redirect("~/Home/Index");
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> JoinGroup(string name,int sum)
        {
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var client = await _db.Clients.Include(c => c.MyGroups).FirstAsync(c => c.Id == currentUserID);
            var @group = await _db.Groups.Include(g => g.Clients).ThenInclude(rp => rp.Client).FirstAsync(g => g.Name == name);            
            if (@group.Clients.FirstOrDefault(rp => rp.Client == client) == null && group.Type == 1 && sum == 1)
            {
                Role_Power cl = new Role_Power { Group = @group, Client = client, ClientId = client.Id, GroupId = @group.Id, R = "Донатер", P = 1 };
                @group.Clients.Add(cl);
                _db.Clients.FirstOrDefault(cr => cr.Id == currentUserID).MyGroups.Add(cl);
                client.PersBudget -= group.EntryFeeDon;
                group.Budget += group.EntryFeeDon;
                _db.Entry(client).State = EntityState.Modified;
                _db.Entry(group).State = EntityState.Modified;
                await _db.SaveChangesAsync();
                return RedirectToAction("OpenGroup", new { name, x=true });
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
                    return RedirectToAction("OpenGroup", new { name, x=true });
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
                    return RedirectToAction("OpenGroup", new { name,x=true });
                }
                if (sum == 3 && client.PersBudget >= group.EntryFeeVIP) {
                    Role_Power cl = new Role_Power { Group = @group, Client = client, ClientId = client.Id, GroupId = @group.Id, R = "VIP", P = Convert.ToInt32(Math.Round(Convert.ToDouble(@group.Clients.Count() *0.25))) };
                    @group.Clients.Add(cl);
                    _db.Clients.FirstOrDefault(cr => cr.Id == currentUserID).MyGroups.Add(cl);
                    client.PersBudget -= group.EntryFeeVIP;
                    group.Budget += group.EntryFeeVIP;
                    _db.Entry(client).State = EntityState.Modified;
                    _db.Entry(group).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    return RedirectToAction("OpenGroup", new { name, x = true });
                }
                if (sum == 4 && client.PersBudget >= group.EntryFeeMod) {
                    Role_Power cl = new Role_Power { Group = @group, Client = client, ClientId = client.Id, GroupId = @group.Id, R = "Mod", P = Convert.ToInt32(Math.Floor(Convert.ToDouble(@group.Clients.Count() *0.5))) };
                    @group.Clients.Add(cl);
                    _db.Clients.FirstOrDefault(cr => cr.Id == currentUserID).MyGroups.Add(cl);
                    client.PersBudget -= group.EntryFeeMod;
                    group.Budget += group.EntryFeeMod;
                    _db.Entry(client).State = EntityState.Modified;
                    _db.Entry(group).State = EntityState.Modified;
                    await _db.SaveChangesAsync();
                    return RedirectToAction("OpenGroup", new { name, x = true });
                }

            }
            return View(@group);
        }

        [Authorize]
        [HttpGet]
        public IActionResult EditRoleClient(string Login,string name)
        {   /*код изменение роли клиента*/
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var rp = _db.Role_Powers.Include(rp=>rp.Client).Include(rp=>rp.Group).First(rp => rp.Client.Login == Login && rp.Group.Name==name);
            if (rp == null)
            {
                return NotFound();
            }
            ViewBag.Name = rp.Group.Name;
            ViewBag.Login = rp.Client.Login;
            ViewBag.Count = _db.Groups.AsNoTracking().Include(g=>g.Clients).ThenInclude(rp=>rp.Client).First(g=>g.Name==name).Clients.Count();
            ViewBag.EditorRole= _db.Role_Powers.Include(rp => rp.Client).Include(rp => rp.Group).First(rp => rp.ClientId == currentUserID && rp.Group.Name == name).R;
            return View(rp);
        }

        [Authorize]
        [HttpPost]
        public  IActionResult EditRoleClient(string name,string login,string Percent, [Bind("R,P")] Role_Power rp)
        {   /*код изменение роли клиента*/
            var c = _db.Role_Powers.AsNoTracking().Include(rp => rp.Client).Include(rp => rp.Group).First(rp => rp.Client.Login == login && rp.Group.Name == name);
            if (rp.R==null )
            {
                ModelState.AddModelError("Role", "Заполнить!");
                var ID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                ViewBag.Name = c.Group.Name;
                ViewBag.Login = c.Client.Login;
                ViewBag.Count = _db.Groups.AsNoTracking().Include(g => g.Clients).ThenInclude(rp => rp.Client).First(g => g.Name == name).Clients.Count();
                ViewBag.EditorRole = _db.Role_Powers.Include(rp => rp.Client).Include(rp => rp.Group).First(rp => rp.ClientId == ID && rp.Group.Name == name).R;
                return View(c);
            }
            if (Percent == null && rp.P==0)
            {
                ModelState.AddModelError("P", "Заполнить!");
                ModelState.AddModelError("Percent", "Заполнить!");
                var ID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                ViewBag.Name = c.Group.Name;
                ViewBag.Login = c.Client.Login;
                ViewBag.Count = _db.Groups.AsNoTracking().Include(g => g.Clients).ThenInclude(rp => rp.Client).First(g => g.Name == name).Clients.Count();
                ViewBag.EditorRole = _db.Role_Powers.Include(rp => rp.Client).Include(rp => rp.Group).First(rp => rp.ClientId == ID && rp.Group.Name == name).R;
                return View(c);
            }

            if (name == null || login==null )
            {
                return NotFound();
            }
            if (c == null)
            {
                return NotFound();
            }
            rp.ClientId = c.ClientId;
            rp.GroupId = c.GroupId;
            rp.Client = c.Client;
            rp.Group = c.Group;
            if (Percent != null)
            {
                rp.Percent = Math.Round(Convert.ToDouble(Percent, CultureInfo.InvariantCulture) *0.01,4);
                rp.P = Convert.ToInt32(Math.Round(Convert.ToDouble(_db.Groups.AsNoTracking().Include(g => g.Clients).ThenInclude(rp => rp.Client).First(g=>g.Id==rp.GroupId).Clients.Count() * rp.Percent)));
            }                  
            
            _db.Entry<Role_Power>(c).State = EntityState.Detached;
            if (rp.R != null || rp.P >= 0)
            {
                try
                {
                    _db.Entry(rp).State = EntityState.Modified;
                    _db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(login,name))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                _db.Entry<Role_Power>(rp).State = EntityState.Detached;
                return RedirectToAction("OpenGroup", new { name });
            }
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewBag.Name = c.Group.Name;
            ViewBag.Login = c.Client.Login;
            ViewBag.Count = _db.Groups.AsNoTracking().Include(g => g.Clients).ThenInclude(rp => rp.Client).First(g => g.Name == name).Clients.Count();
            ViewBag.EditorRole = _db.Role_Powers.Include(rp => rp.Client).Include(rp => rp.Group).First(rp => rp.ClientId == currentUserID && rp.Group.Name == name).R;
            return View(c);
        }
        private bool ClientExists(string login,string name)
        {
            return _db.Groups.First(g => g.Name == name).Clients.Any(e => e.Client.Login == login);
        }
        [Authorize]
        public async Task<IActionResult> BanClient(string Login, string name)
        {   /*код бан клиента*/
            var client = await _db.Role_Powers.Include(rp => rp.Client).FirstAsync(rp => rp.Client.Login == Login && rp.GroupId == name);
            client.R = "Забанен";
            client.P = 0;
            _db.Entry(client).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return RedirectToAction("OpenGroup", new { name });
        }
        [Authorize]
        // GET: Groups/Create
        public IActionResult Create()
        {            
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
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
                    var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
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
                    return Redirect("~/Home/Index");

            }
            return View(@group);
        }

        // GET: Groups/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(string name)
        {
            if (name == null)
            {
                return NotFound();
            }

            var @group = await _db.Groups.FirstAsync(g=>g.Name==name);
            if (@group == null)
            {
                return NotFound();
            }
            ViewBag.Name = group.Name;
            return View(@group);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string name, [Bind("Id,Name,Type,CloseCall,EntryFeeDon,EntryFeeUser,EntryFeeVIP,EntryFeeMod,MinPlus,MinMinus")] Group @group)
        {
            if (name != @group.Name)
            {
                return NotFound();
            }
            var gg = _db.Groups.AsNoTracking().Include(g=>g.Clients).ThenInclude(rp=>rp.Client).Include(g=>g.Transactions).ThenInclude(t=>t.Votes).First(g => g.Id == group.Id);
            @group.Id = gg.Id;
            if (_db.Groups.Any(g => g.Name == @group.Name && g.Id != @group.Id) == true)
            {
                ModelState.AddModelError("Name", "Такое имя занято!");
                return View(@group);
            }
            @group.Budget =gg.Budget;
            @group.Clients = gg.Clients;
            @group.Transactions = gg.Transactions;
            _db.Entry<Group>(gg).State = EntityState.Detached;
            if (ModelState.IsValid)
            {
                try
                {
                    _db.Entry(@group).State = EntityState.Modified;
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
                return Redirect("~/Home/Index");
            }
            return View(@group);
        }

        // GET: Groups/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(string name)
        {
            if (name == null)
            {
                return NotFound();
            }

            var @group = await _db.Groups
                .FirstOrDefaultAsync(m => m.Name == name);
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
        public async Task<IActionResult> DeleteConfirmed(string name)
        {
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var @group = await _db.Groups.FirstAsync(g=>g.Name==name);
            var client = _db.Groups.Include(g => g.Clients).ThenInclude(rp=>rp.Client).First(g => g.Name == name)
                .Clients.FirstOrDefault(rp=>rp.ClientId== currentUserID && rp.R=="Создатель").Client;

            if (client != null)
            {
                client.PersBudget += group.Budget;
                _db.Entry(client).State = EntityState.Modified;
                _db.Groups.Remove(@group);
                await _db.SaveChangesAsync();
            }
            return Redirect("~/Home/Index");
        }

        private bool GroupExists(string name)
        {
            return _db.Groups.Any(e => e.Name == name);
        }
    }
}
