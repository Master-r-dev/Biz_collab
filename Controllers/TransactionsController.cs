using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Biz_collab.Data;
using Biz_collab.Models;
using System.Security.Claims;

namespace Biz_collab.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public TransactionsController(ApplicationDbContext context)
        {
            _db = context;
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _db.Transactions.Include(t => t.Client).Include(t => t.Group);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _db.Transactions
                .Include(t => t.Client)
                .Include(t => t.Group)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transactions/Create
        public IActionResult Create(string id)
        {   
            var group = _db.Groups.Include(g=>g.Transactions).First(g => g.Id == id);
            ViewBag.Budget =group.Budget;
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var client = _db.Clients.Find(currentUserID);
            ViewBag.PersBudget = client.PersBudget;
            Transaction transaction = new Transaction { ClientId= currentUserID, GroupId = id , Client=client,Group=group, Id=group.Transactions.Count+1 };
            return View(transaction);
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                ClaimsPrincipal currentUser = this.User;
                var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
                var currentUserLog = currentUser.FindFirst(ClaimTypes.Name).Value;
                transaction.ClientId = currentUserID;

                // ниже происходит автоматически если владелец.Иначе транзакция ждет подтверждения
                // по данному id группы в которой происходит транзакция нужно в бд найти эту группу и изменить в ней поле budget
                var gc = _db.Role_Powers.Include(rp=>rp.Client).Include(rp=>rp.Group).First(cr => cr.ClientId == transaction.ClientId && cr.GroupId == transaction.GroupId);
                if (gc.Group.Type == 1 || gc.R == "Создатель")
                {
                    transaction.StartTime = DateTime.Now;
                    transaction.Status = true;

                    //трата бюджета  на внешние источники (тоесть физ у.е. потраченны и обновляется данные на сайте для документации)
                    if (transaction.OperationType == 1 && transaction.Amount <= gc.Group.Budget)
                    {
                        gc.Group.Budget -= transaction.Amount;
                        gc.Group.Transactions.Add(transaction);
                        _db.Entry(gc.Group).State = EntityState.Modified;
                        _db.Transactions.Add(transaction);
                    }
                    //перевод с счета группы на счет пользователя
                    else if (transaction.OperationType == 2 && transaction.Amount <= gc.Group.Budget && gc.Client.PersBudget+ transaction.Amount < 2147483647)
                    {
                        gc.Group.Budget -= transaction.Amount;
                        gc.Group.Transactions.Add(transaction);
                        _db.Entry(gc.Group).State = EntityState.Modified;
                        gc.Client.PersBudget += transaction.Amount;
                        gc.Client.MyTransactions.Add(transaction);
                        _db.Entry(gc.Client).State = EntityState.Modified;
                        _db.Transactions.Add(transaction);
                    }
                    //перевод с счета пользователя  на счет группы
                    else if (transaction.OperationType == 3 && transaction.Amount <= gc.Client.PersBudget)
                    {
                        gc.Group.Budget += transaction.Amount;
                        gc.Group.Transactions.Add(transaction);
                        _db.Entry(gc.Group).State = EntityState.Modified;
                        gc.Client.PersBudget -= transaction.Amount;
                        gc.Client.MyTransactions.Add(transaction);
                        _db.Entry(gc.Client).State = EntityState.Modified;
                        _db.Transactions.Add(transaction);
                    }
                }
                else
                {
                    if (transaction.OperationType == 1 && transaction.Amount <= gc.Group.Budget)
                    {
                        transaction.Status = false;
                        transaction.YesPercent = gc.P / _db.Role_Powers.Where(rp => rp.GroupId == transaction.GroupId).Count() * 100;
                        transaction.NoPercent = 0;
                        gc.Group.Transactions.Add(transaction);
                        gc.Client.MyTransactions.Add(transaction);
                        _db.Transactions.Add(transaction);
                    }
                    //перевод с счета группы на счет пользователя
                    else if (transaction.OperationType == 2 && transaction.Amount <= gc.Group.Budget)
                    {
                        transaction.Status = false;
                        transaction.YesPercent = gc.P / _db.Role_Powers.Where(rp => rp.GroupId == transaction.GroupId).Count() * 100;
                        transaction.NoPercent = 0;
                        gc.Group.Transactions.Add(transaction);
                        gc.Client.MyTransactions.Add(transaction);
                        _db.Transactions.Add(transaction);
                    }
                    if (transaction.OperationType == 3 && transaction.Amount <= gc.Client.PersBudget)
                    {
                        transaction.StartTime = DateTime.Now;
                        transaction.Status = true;
                        gc.Group.Budget += transaction.Amount;
                        gc.Group.Transactions.Add(transaction);                        
                        _db.Entry(gc.Group).State = EntityState.Modified;
                        gc.Client.PersBudget -= transaction.Amount;
                        gc.Client.MyTransactions.Add(transaction);
                        _db.Entry(gc.Client).State = EntityState.Modified;
                        _db.Transactions.Add(transaction);
                    }
                }
                await _db.SaveChangesAsync();
                return Redirect("~/Home/Index");

            }
            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _db.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            ViewData["ClientId"] = new SelectList(_db.Clients, "Id", "Id", transaction.ClientId);
            ViewData["GroupId"] = new SelectList(_db.Groups, "Id", "Id", transaction.GroupId);
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,GroupId,Amount,OperationType,Explanation")] Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _db.Update(transaction);
                    await _db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.Id))
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
            ViewData["ClientId"] = new SelectList(_db.Clients, "Id", "Id", transaction.ClientId);
            ViewData["GroupId"] = new SelectList(_db.Groups, "Id", "Id", transaction.GroupId);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _db.Transactions
                .Include(t => t.Client)
                .Include(t => t.Group)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _db.Transactions.FindAsync(id);
            _db.Transactions.Remove(transaction);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _db.Transactions.Any(e => e.Id == id);
        }

        public async Task<IActionResult> VoteYes(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _db.Transactions
                .Include(t => t.Client)
                .Include(t => t.Group)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        public async Task<IActionResult> VoteNo(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _db.Transactions
                .Include(t => t.Client)
                .Include(t => t.Group)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }
    }
}
