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
        public async Task<IActionResult> Container(string id) //Сама группа
        {
            if (id == null)
            {
                return NotFound();
            }
            var @group = await _db.Groups.Include(g=>g.Transactions).ThenInclude(t=>t.Votes).ThenInclude(v=>v.Client).Include(g => g.Clients).ThenInclude(rp=>rp.Client).Where(rp => rp.Id == id)
                .ToListAsync();            
            if (@group == null)
            {
                return NotFound();
            }

            return View(@group);
        }
        [Authorize]
        public IActionResult EditRoleClient(string login)
        {
            /*код изменение роли клиента*/
            return View();
        }
       [Authorize]
        public IActionResult BanClient(string login)
        {
            /*код банна клиента*/
            return View();
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
                Role_Power cl = new Role_Power { Group = @group, Client = client, ClientId = client.Id, GroupId = @group.Id, R = "Создатель", P = 2147483646 };//overkill value ;)
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
