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
        private readonly ApplicationDbContext _context;

        public GroupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Groups
        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Groups.ToListAsync());
        }
        [Authorize]
        // GET: Groups/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@group == null)
            {
                return NotFound();
            }

            return View(@group);
        }
        [Authorize]
        // GET: Groups/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Groups/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Budget,Type")] Group @group)
        {
            if (ModelState.IsValid)
            {
                //убедится что получает на вход текущего клиента
                ClaimsPrincipal currentUser = this.User;
                var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
                @group.Id = Guid.NewGuid().ToString();
                //находим клиента который создает и добовляем его в группу ,даем роль создателя 
                var client = _context.Clients.Include(c => c.MyGroups).Where(cr => cr.Id == currentUserID).FirstOrDefault();
                GroupClient cl= new GroupClient { Client = client };
                cl.Client.Role= "Создатель";
                @group.Clients.Add(cl);
                //у этого клиента вычитаем сумму которая выдалась на группу
                if(client.PersBudget<= @group.Budget)  _context.Clients.Where(cr => cr.Id == currentUserID).FirstOrDefault().PersBudget-= @group.Budget;
                _context.Add(@group);
                //добавляем созданую группу к создателю клиенту
                _context.Clients.Where(cr => cr.Id == currentUserID).FirstOrDefault().MyGroups.Add(new GroupClient { Group = @group });
                await _context.SaveChangesAsync();
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

            var @group = await _context.Groups.FindAsync(id);
            if (@group == null)
            {
                return NotFound();
            }
            return View(@group);
        }

        // POST: Groups/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Name,Budget,Type")] Group @group)
        {
            if (id != @group.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@group);
                    await _context.SaveChangesAsync();
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

            var @group = await _context.Groups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@group == null)
            {
                return NotFound();
            }

            return View(@group);
        }

        // POST: Groups/Delete/5
        [Authorize] // [Authorize(Roles ="Владелец")] пример
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var @group = await _context.Groups.FindAsync(id);
            _context.Groups.Remove(@group);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GroupExists(string id)
        {
            return _context.Groups.Any(e => e.Id == id);
        }
    }
}
