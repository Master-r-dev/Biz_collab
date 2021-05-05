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
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics.CodeAnalysis;

namespace Biz_collab.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _db;
        //protected readonly IHubContext<ChatHub> _chatHub;

        public GroupsController(ApplicationDbContext context/*,[NotNull] IHubContext<ChatHub> chatHub*/)
        {
           // _chatHub = chatHub;
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
            if (client.PersBudget >= group.EntryFeeDon || (client.PersBudget >= group.EntryFeeUser && group.EntryFeeUser!=-1) || (client.PersBudget >= group.EntryFeeMod && group.EntryFeeMod != -1) || (client.PersBudget >= group.EntryFeeVIP && group.EntryFeeVIP != -1))
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
            var client = await _db.Clients.AsNoTracking().Include(c=>c.MyGroups).FirstAsync(c=>c.Id== currentUserID);            
            var @group = await _db.Groups.Include(g => g.Clients).ThenInclude(rp=>rp.Client).Include(g=>g.Messages).FirstAsync(g => g.Name == name);
            if (@group.Clients.FirstOrDefault(rp => rp.ClientId == client.Id && rp.R == "Забанен") != null)
            {
                return RedirectToAction("BannedInGroup", new { name });
            }
            if (@group == null)
            {
                return NotFound();
            }

            if (@group.Clients.FirstOrDefault(rp=>rp.ClientId == client.Id) == null)
            {
                return RedirectToAction("JoinGroup", new { name });
            }
            var trans = _db.Transactions.Include(t => t.Client).ThenInclude(c => c.MyGroups).ThenInclude(rp => rp.Group).Include(t => t.Votes).ThenInclude(v => v.Client).Where(t => t.GroupId == @group.Id);
            if (x)
            {
                if (trans != null)
                {
                    foreach (var i in trans)
                    {
                        if (i.Status == false)
                        {
                            int YesCounter = _db.Votes.Where(v => v.TransactionId == i.Id && v.V == true).Sum(v => v.P);
                            int NoCounter = _db.Votes.Where(v => v.TransactionId == i.Id && v.V == false).Sum(v => v.P);
                            i.YesPercent = ((float)YesCounter / @group.Clients.Count()) * 100.0f;
                            i.NoPercent = ((float)NoCounter / @group.Clients.Count()) * 100.0f;
                            _db.Entry(i).State = EntityState.Modified;
                        }
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
                foreach (var i in @group.Clients.Where(rp => rp.Percent != null))
                {
                    i.P = Convert.ToInt32(Math.Round(Convert.ToDouble(@group.Clients.Count() * i.Percent)));
                    _db.Entry(i).State = EntityState.Modified;
                }
                @group.Clients.FirstOrDefault(rp => rp.R == "Creator").P = @group.Clients.Count();
                _db.Entry(group).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            } //новый пользователь

            if (trans == null)
            {
                return View(@group);
            }
            var amounts = new int[trans.Where(t => t.Status == true).Count()];
            var dates = new int[trans.Where(t => t.Status == true).Count()][];
            var total_spent = 0;
            var total_recieved = 0;
            int k = 0;
            foreach (var t in trans.Where(t => t.Status == true).OrderBy(tr => tr.StartTime))
            {
                if (t.OperationType == 3)
                {
                    amounts[k] = t.Amount;
                    total_recieved += t.Amount;

                }
                else
                {
                    amounts[k] = -t.Amount;
                    total_spent += t.Amount;
                }
                dates[k++] = new int[] { t.StartTime.Year, t.StartTime.Month, t.StartTime.Day };

            }
            ViewBag.trans_amounts = JsonSerializer.Serialize(amounts);
            ViewBag.trans_dates = JsonSerializer.Serialize(dates);
            ViewBag.total_spent = total_spent;
            ViewBag.total_recieved = total_recieved;

            var clients = new string[group.Clients.Count()];
            var clients_trans = new int[group.Clients.Count()];
            k = 0;
            foreach (var c in group.Clients)
            {
                clients[k] = c.Client.Login;
                clients_trans[k++] = trans.Where(t => t.ClientId == c.Client.Id && t.Status == true).Count();
            }
            ViewBag.clients = JsonSerializer.Serialize(clients);
            ViewBag.clients_trans = JsonSerializer.Serialize(clients_trans);

            ViewBag.Count = trans.Count();
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
                trans = trans.Where(s => s.Client.Login.Contains(searchString) || s.Client.MyGroups.FirstOrDefault(rp=>rp.GroupId==s.GroupId && rp.R==searchString)!=null || s.Explanation.Contains(searchString));
            }
            ViewData["TimeSortParm"] = sortOrder == "time_desc" ? "Time" : "time_desc";
            ViewData["ClientNameSortParm"] = sortOrder == "Name" ? "name_desc" : "Name";
            ViewData["OperationTypeSortParm"] = sortOrder == "OperationType" ? "optype_desc" : "OperationType";
            ViewData["AmountSortParm"] = sortOrder == "Amount" ? "amount_desc" : "Amount";
            trans = sortOrder switch
            {
                "OperationType" => trans.OrderBy(s => s.OperationType),
                "Name" => trans.OrderBy(s => s.Client.Login),
                "Amount" => trans.OrderBy(s => s.Amount),
                "amount_desc" => trans.OrderByDescending(s => s.Amount),
                "Time" => trans.OrderBy(s => s.StartTime),
                "time_desc" => trans.OrderByDescending(s => s.StartTime),
                "optype_desc" => trans.OrderByDescending(s => s.OperationType),
                "name_desc" => trans.OrderByDescending(s => s.Client.Login),
                _ => trans.OrderByDescending(s => s.StartTime),
            };
            int pageSize = 8;            
            ViewBag.Transactions = await PaginatedList<Transaction>.CreateAsync(trans, pageNumber ?? 1, pageSize);            
            return View(@group);
        }
        [Authorize]
        public IActionResult BannedInGroup(string name)
        {
            ViewBag.Name = _db.Groups.AsNoTracking().First(g => g.Name == name).Name;
            return PartialView();
        }
        [Authorize]
        [HttpGet]
        public IActionResult JoinGroup(string name)
        {
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var client =  _db.Clients.AsNoTracking().First(c => c.Id == currentUserID);
            ViewBag.PersBudget =  client.PersBudget;
            var group =  _db.Groups.AsNoTracking().First(g => g.Name == name);
            if (group.EntryFeeDon!=-1) {
                if (client.PersBudget >= group.EntryFeeDon || client.PersBudget >= group.EntryFeeUser || client.PersBudget >= group.EntryFeeMod || client.PersBudget >= group.EntryFeeVIP) {
                    return PartialView(group);
                } 
            }
            return Redirect("~/Home/Index");
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> JoinGroup(string name,int sum)
        {
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;            
            var @group = await _db.Groups.Include(g => g.Clients).ThenInclude(rp => rp.Client).ThenInclude(c=>c.MutedList).FirstAsync(g => g.Name == name);
            var client = await _db.Clients.Include(c => c.MyGroups).FirstAsync(c => c.Id == currentUserID);
            if (@group.Clients.FirstOrDefault(rp => rp.Client == client) == null && group.Type == 1 && sum == 1)
            {
                Role_Power cl = new Role_Power {
                    Group = @group,
                    Client = client,
                    ClientId = client.Id,
                    GroupId = @group.Id,
                    R = "Don",
                    P = 1
                };
                @group.Clients.Add(cl);
                _db.Clients.FirstOrDefault(cr => cr.Id == currentUserID).MyGroups.Add(cl);
                client.PersBudget -= group.EntryFeeDon;
                group.Budget += group.EntryFeeDon;
                _db.Entry(client).State = EntityState.Modified;
                _db.Entry(group).State = EntityState.Modified;
                if (!@group.Clients.FirstOrDefault(rp => rp.R == "Creator").Client.MutedList.Any(m => m.MutedName == @group.Name))
                {
                    Notification newUser = new Notification {
                        ClientId = @group.Clients.FirstOrDefault(rp => rp.R == "Creator").ClientId,
                        NotiHeader = "Новый донатер",
                        NotiBody = " Добавился пользователь-" + client.Login +" в вашей группе:" + @group.Name ,
                        IsRead = false,
                        Url = "../Groups/OpenGroup?name=" + @group.Name
                    };
                    _db.Notifications.Add(newUser);
                }
                await _db.SaveChangesAsync();
                return RedirectToAction("OpenGroup", new { name, x=true });
            }
            else if (@group.Clients.FirstOrDefault(rp => rp.Client == client) == null && group.Type == 2)
            {               
                if (sum == 1 && client.PersBudget>=group.EntryFeeDon) {
                    Role_Power cl = new Role_Power {
                        Group = @group,
                        Client = client,
                        ClientId = client.Id,
                        GroupId = @group.Id,
                        R = "Don",
                        P = 1
                    };
                    @group.Clients.Add(cl);
                    _db.Clients.FirstOrDefault(cr => cr.Id == currentUserID).MyGroups.Add(cl);
                    client.PersBudget -= group.EntryFeeDon;
                    group.Budget += group.EntryFeeDon;
                    _db.Entry(client).State = EntityState.Modified;
                    _db.Entry(group).State = EntityState.Modified;
                    if (!@group.Clients.FirstOrDefault(rp => rp.R == "Creator").Client.MutedList.Any(m => m.MutedName == @group.Name))
                    {
                        Notification newUser = new Notification {
                            ClientId = @group.Clients.FirstOrDefault(rp => rp.R == "Creator").ClientId,
                            NotiHeader = "Новый донатер",
                            NotiBody = " Добавился пользователь-" + client.Login + " в вашей группе:" + @group.Name,
                            IsRead = false,
                            Url = ""
                        };
                        _db.Notifications.Add(newUser);
                    }
                    await _db.SaveChangesAsync();
                    return RedirectToAction("OpenGroup", new { name, x=true });
                }
                if (sum == 2 && client.PersBudget >= group.EntryFeeUser) {
                    Role_Power cl = new Role_Power {
                        Group = @group,
                        Client = client,
                        ClientId = client.Id,
                        GroupId = @group.Id,
                        R = "User",
                        P = 1
                    };
                    @group.Clients.Add(cl);
                    _db.Clients.FirstOrDefault(cr => cr.Id == currentUserID).MyGroups.Add(cl);
                    client.PersBudget -= group.EntryFeeUser;
                    group.Budget += group.EntryFeeUser;
                    _db.Entry(client).State = EntityState.Modified;
                    _db.Entry(group).State = EntityState.Modified;
                    if (!@group.Clients.FirstOrDefault(rp => rp.R == "Creator").Client.MutedList.Any(m => m.MutedName == @group.Name))
                    {
                        Notification newUser = new Notification {
                            ClientId = @group.Clients.FirstOrDefault(rp => rp.R == "Creator").ClientId,
                            NotiHeader = "Новый участник",
                            NotiBody = " Добавился пользователь-" + client.Login + " в вашей группе:" + @group.Name,
                            IsRead = false,
                            Url = ""
                        };
                        _db.Notifications.Add(newUser);
                    }
                    await _db.SaveChangesAsync();
                    return RedirectToAction("OpenGroup", new { name,x=true });
                }
                if (sum == 3 && client.PersBudget >= group.EntryFeeVIP) {
                    Role_Power cl = new Role_Power {
                        Group = @group,
                        Client = client,
                        ClientId = client.Id,
                        GroupId = @group.Id,
                        R = "VIP",
                        P = Convert.ToInt32(Math.Round(Convert.ToDouble(@group.Clients.Count() * 0.25)))
                    };
                    @group.Clients.Add(cl);
                    _db.Clients.FirstOrDefault(cr => cr.Id == currentUserID).MyGroups.Add(cl);
                    client.PersBudget -= group.EntryFeeVIP;
                    group.Budget += group.EntryFeeVIP;
                    _db.Entry(client).State = EntityState.Modified;
                    _db.Entry(group).State = EntityState.Modified;
                    if (!@group.Clients.FirstOrDefault(rp => rp.R == "Creator").Client.MutedList.Any(m => m.MutedName == @group.Name))
                    {
                        Notification newUser = new Notification {
                            ClientId = @group.Clients.FirstOrDefault(rp => rp.R == "Creator").ClientId,
                            NotiHeader = "Новый VIP",
                            NotiBody = " Добавился пользователь-" + client.Login + " в вашей группе:" + @group.Name,
                            IsRead = false,
                            Url = ""
                        };
                        _db.Notifications.Add(newUser);
                    }
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
                    if (!@group.Clients.FirstOrDefault(rp => rp.R == "Creator").Client.MutedList.Any(m => m.MutedName == @group.Name))
                    {
                        Notification newUser = new Notification {
                            ClientId = @group.Clients.FirstOrDefault(rp => rp.R == "Creator").ClientId,
                            NotiHeader = "Новый модератор",
                            NotiBody = " Добавился пользователь-" + client.Login + " в вашей группе:" + @group.Name,
                            IsRead = false,
                            Url = ""
                        };
                        _db.Notifications.Add(newUser);
                    }
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
            ViewBag.Name = name;
            ViewBag.Login = rp.Client.Login;
            ViewBag.Count = _db.Groups.AsNoTracking().Include(g=>g.Clients).ThenInclude(rp=>rp.Client).First(g=>g.Name==name).Clients.Count();
            ViewBag.EditorRole= _db.Role_Powers.AsNoTracking().Include(rp => rp.Client).Include(rp => rp.Group).FirstOrDefault(rp => rp.ClientId == currentUserID && rp.Group.Name == name).R;
            return PartialView(rp);
        }

        [Authorize]
        [HttpPost]
        public  IActionResult EditRoleClient(string name,string login,string Percent, [Bind("R,P")] Role_Power rp)
        {   /*код изменение роли клиента*/
            var c = _db.Role_Powers.AsNoTracking().Include(rp => rp.Client).Include(rp => rp.Group).FirstOrDefault(rp => rp.Client.Login == login && rp.Group.Name == name);
            if (rp.R==null )
            {
                ModelState.AddModelError("Role", "Заполнить!");
                var ID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                ViewBag.Name = name;
                ViewBag.Login = c.Client.Login;
                ViewBag.Count = _db.Groups.AsNoTracking().Include(g => g.Clients).ThenInclude(rp => rp.Client).FirstOrDefault(g => g.Name == name).Clients.Count();
                ViewBag.EditorRole = _db.Role_Powers.Include(rp => rp.Client).Include(rp => rp.Group).FirstOrDefault(rp => rp.ClientId == ID && rp.Group.Name == name).R;
                return PartialView(c);
            }
            if (Percent == null && rp.P==0)
            {
                ModelState.AddModelError("P", "Заполнить!");
                ModelState.AddModelError("Percent", "Заполнить!");
                var ID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                ViewBag.Name = name;
                ViewBag.Login = c.Client.Login;
                ViewBag.Count = _db.Groups.AsNoTracking().Include(g => g.Clients).ThenInclude(rp => rp.Client).First(g => g.Name == name).Clients.Count();
                ViewBag.EditorRole = _db.Role_Powers.Include(rp => rp.Client).Include(rp => rp.Group).FirstOrDefault(rp => rp.ClientId == ID && rp.Group.Name == name).R;
                return PartialView(c);
            }

            if (name == null || login==null )
            {
                return NotFound();
            }
            if (c == null)
            {
                return NotFound();
            }
            rp.Client = c.Client;
            rp.ClientId = c.ClientId;
            rp.GroupId = c.GroupId;
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
                    if (!rp.Client.MutedList.Any(m => m.MutedName == name || m.MutedName == this.User.FindFirst(ClaimTypes.Name).Value))
                    {
                        if (rp.Percent == null)
                        {
                            Notification changedUser = new Notification
                            {
                                ClientId = rp.ClientId,
                                NotiHeader = "У Вас изменил роль на:" + rp.R + " с силой:" + rp.P,
                                NotiBody = "Пользователь-"+ this.User.FindFirst(ClaimTypes.Name).Value + " в группе:" + name,
                                IsRead = false,
                                Url = "../Groups/OpenGroup?name=" + name
                            };
                            _db.Notifications.Add(changedUser);
                        }
                        else
                        {
                            Notification changedUser = new Notification
                            {
                                ClientId = rp.ClientId,
                                NotiHeader = "У Вас изменил роль на:" + rp.R + " с силой:" + rp.P + " и процентом:" + rp.Percent,
                                NotiBody = "Пользователь-" + this.User.FindFirst(ClaimTypes.Name).Value + " в группе:" + name,                                
                                IsRead = false,
                                Url = "../Groups/OpenGroup?name=" + name
                            };
                            _db.Notifications.Add(changedUser);
                        }
                    }
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
            ViewBag.Name = name;
            ViewBag.Login = c.Client.Login;
            ViewBag.Count = _db.Groups.AsNoTracking().Include(g => g.Clients).ThenInclude(rp => rp.Client).First(g => g.Name == name).Clients.Count();
            ViewBag.EditorRole = _db.Role_Powers.Include(rp => rp.Client).Include(rp => rp.Group).FirstOrDefault(rp => rp.ClientId == currentUserID && rp.Group.Name == name).R;
            return PartialView(c);
        }
        [Authorize]
        [HttpGet]
        public IActionResult InviteClient(string name)
        {   
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewBag.Name = name;
            ViewBag.Count = _db.Groups.AsNoTracking().Include(g => g.Clients).ThenInclude(rp => rp.Client).First(g => g.Name == name).Clients.Count();
            ViewBag.InviterRole = _db.Role_Powers.AsNoTracking().Include(rp => rp.Client).Include(rp => rp.Group).FirstOrDefault(rp => rp.ClientId == currentUserID && rp.Group.Name == name).R;
            return PartialView();
        }
        [Authorize]
        [HttpPost]
        public IActionResult InviteClient(string name, string login, string Percent, [Bind("R,P")] Role_Power rp)
        {   /*код приглашения клиента*/
            if (!ClientExists(login, name) && _db.Clients.AsNoTracking().Any(c => c.Login == login))
            {
                if (rp.R == null)
                {
                    ModelState.AddModelError("Role", "Заполнить!");
                    var ID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                    ViewBag.Name = name;
                    ViewBag.Count = _db.Groups.AsNoTracking().Include(g => g.Clients).ThenInclude(rp => rp.Client).First(g => g.Name == name).Clients.Count();
                    ViewBag.InviterRole = _db.Role_Powers.AsNoTracking().Include(rp => rp.Client).Include(rp => rp.Group).FirstOrDefault(rp => rp.ClientId == ID && rp.Group.Name == name).R;
                    return PartialView();
                }
                if (Percent == null && rp.P == 0)
                {
                    ModelState.AddModelError("P", "Заполнить!");
                    ModelState.AddModelError("Percent", "Заполнить!");
                    var ID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
                    ViewBag.Name = name;
                    ViewBag.Count = _db.Groups.AsNoTracking().Include(g => g.Clients).ThenInclude(rp => rp.Client).First(g => g.Name == name).Clients.Count();
                    ViewBag.InviterRole = _db.Role_Powers.AsNoTracking().Include(rp => rp.Client).Include(rp => rp.Group).FirstOrDefault(rp => rp.ClientId == ID && rp.Group.Name == name).R;
                    return PartialView();
                }

                if (name == null || login == null)
                {
                    return NotFound();
                }
                rp.Client = _db.Clients.AsNoTracking().Include(c=>c.MutedList).First(c => c.Login == login);
                rp.ClientId = _db.Clients.AsNoTracking().First(c=>c.Login==login).Id;
                rp.GroupId = _db.Groups.AsNoTracking().First(g => g.Name == name).Id;
                if (Percent != null)
                {
                    rp.Percent = Math.Round(Convert.ToDouble(Percent, CultureInfo.InvariantCulture) * 0.01, 4);
                    rp.P = Convert.ToInt32(Math.Round(Convert.ToDouble((_db.Groups.AsNoTracking().Include(g => g.Clients).ThenInclude(rp => rp.Client).First(g => g.Id == rp.GroupId).Clients.Count()+1) * rp.Percent)));
                }
                if (rp.R != null || rp.P >= 0)
                {
                    try
                    {
                        if (!rp.Client.MutedList.Any(m => m.MutedName == name || m.MutedName == this.User.FindFirst(ClaimTypes.Name).Value))
                        {
                            if (rp.Percent == null)
                            {
                            
                                Notification inviteUser = new Notification
                                {
                                    ClientId = rp.ClientId,
                                    NotiHeader = "Вас пригласил на роль:" + rp.R + " с силой:" + rp.P,
                                    NotiBody = "Пользователь-" + this.User.FindFirst(ClaimTypes.Name).Value + " в группе:" + name,
                                    IsRead = false,
                                    Url = ""
                                };
                                _db.Notifications.Add(inviteUser);
                            }
                            else
                            {
                                Notification inviteUser = new Notification
                                {
                                    ClientId = rp.ClientId,
                                    NotiHeader = "Вас пригласил на роль:" + rp.R + " с силой:" + rp.P + " и процентом:" + rp.Percent,
                                    NotiBody = "Пользователь-" + this.User.FindFirst(ClaimTypes.Name).Value + " в группе:" + name,                                    
                                    IsRead = false,
                                    Url = ""
                                };
                                _db.Notifications.Add(inviteUser);
                            }
                        }
                        _db.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (ClientExists(login, name))
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
            }
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            ViewBag.Name = name;
            ViewBag.Count = _db.Groups.AsNoTracking().Include(g => g.Clients).ThenInclude(rp => rp.Client).First(g => g.Name == name).Clients.Count();
            ViewBag.InviterRole = _db.Role_Powers.AsNoTracking().Include(rp => rp.Client).Include(rp => rp.Group).FirstOrDefault(rp => rp.ClientId == currentUserID && rp.Group.Name == name).R;
            return PartialView();
        }
        
        private bool ClientExists(string login,string name)
        {
            return _db.Groups.First(g => g.Name == name).Clients.Any(e => e.Client.Login == login);
        }
        [Authorize]
        public async Task<IActionResult> BanClient(string Login, string name)
        {   /*код бан клиента*/
            var client = await _db.Role_Powers.Include(rp => rp.Client).Include(rp=>rp.Group).FirstAsync(rp => rp.Client.Login == Login && rp.Group.Name == name);
            client.R = "Забанен";
            client.P = 0;
            client.Percent = 0;
            _db.Entry(client).State = EntityState.Modified;
            if (!client.Client.MutedList.Any(m => m.MutedName == name || m.MutedName == this.User.FindFirst(ClaimTypes.Name).Value))
            {
                Notification BanClient = new Notification
                {
                    ClientId = client.ClientId,
                    NotiHeader = "Вы были заблокированы.Осмыслите свое поведение.",
                    NotiBody = "Пользователем-" + this.User.FindFirst(ClaimTypes.Name).Value + " в группе:" + name,
                    IsRead = false,
                    Url = ""
                };
                _db.Notifications.Add(BanClient);
            }
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
                    Role_Power cl = new Role_Power { Group = @group, Client = client, ClientId = client.Id, GroupId = @group.Id, R = "Creator", P = 2 };
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
                    foreach (var rp in @gg.Clients)
                    {
                        if (rp.ClientId == this.User.FindFirst(ClaimTypes.NameIdentifier).Value) { continue; }
                        if (!rp.Client.MutedList.Any(m => m.MutedName == @group.Name || m.MutedName == this.User.FindFirst(ClaimTypes.Name).Value))
                        {
                            Notification editGroup = new Notification
                            {
                                ClientId = rp.ClientId,
                                NotiHeader = "Изменена группа",
                                NotiBody = "Пользователем-"+ this.User.FindFirst(ClaimTypes.Name) + " .Изменения находятся в деталях группы:"+ name,
                                IsRead = false,
                                Url = "../Groups/OpenGroup?name=" + group.Name
                            };
                            _db.Notifications.Add(editGroup);
                        }
                    }
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
        public IActionResult Delete(string name)
        {
            if (name == null)
            {
                return NotFound();
            }

            var group = _db.Groups
                .FirstOrDefault(m => m.Name == name);
            if (group == null)
            {
                return NotFound();
            }

            return PartialView(group);
        }

        // POST: Groups/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string name)
        {
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var @group = await _db.Groups.Include(g=>g.Clients).ThenInclude(rp => rp.Client).FirstAsync(g=>g.Name==name);
            var client = @group.Clients.FirstOrDefault(rp=>rp.ClientId== currentUserID && rp.R=="Creator").Client;

            if (client != null)
            {
                client.PersBudget += group.Budget;
                _db.Entry(client).State = EntityState.Modified;
                foreach (var rp in @group.Clients)
                {
                    if (rp.ClientId == client.Id) { continue; }
                    if (!rp.Client.MutedList.Any(m => m.MutedName == name || m.MutedName == this.User.FindFirst(ClaimTypes.Name).Value))
                    {
                        Notification deleteGroup = new Notification
                        {
                            ClientId = rp.ClientId,
                            NotiHeader = "Удалена группа " + name,
                            NotiBody = "Все средства у ее создателя-" + client.Login,
                            IsRead = false,
                            Url = ""
                        };
                        _db.Notifications.Add(deleteGroup);
                    }
                }

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
