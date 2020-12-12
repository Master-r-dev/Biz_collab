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
        // GET: Transactions/Details/5
        public IActionResult Details(string id)
        {

            if (id == null)
            {
                return NotFound();
            }

            Transaction transaction = _db.Transactions
                .Include(t => t.Client)
                .Include(t => t.Group)
                .FirstOrDefault(m => m.Id == id);
            ViewBag.Name = transaction.Group.Name;
            if (transaction != null)
            {
                return  PartialView(transaction);
            }
            return NotFound();
        }

        // GET: Transactions/Create
        public IActionResult Create(string name)
        {
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var rp = _db.Role_Powers.AsNoTracking().Include(rp => rp.Client).Include(rp => rp.Group).First(rp=>rp.ClientId== currentUserID && rp.Group.Name== name);
            if (rp.R == "Забанен")
            {
                return Redirect("~/Home/Index");
            }
            ViewBag.MinPlus = rp.Group.MinPlus;
            ViewBag.MinMinus = rp.Group.MinMinus;
            ViewBag.Name =name;
            ViewBag.Budget =rp.Group.Budget;
            ViewBag.PersBudget = rp.Client.PersBudget;
            ViewBag.Role = rp.R;            
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, [Bind("Amount,OperationType,Explanation")] Transaction transaction)
        {
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var gc = _db.Role_Powers.Include(rp => rp.Client).Include(rp => rp.Group).First(rp => rp.ClientId == currentUserID && rp.Group.Name == name);
            transaction.ClientId = currentUserID;
            transaction.GroupId = gc.GroupId;
            transaction.Client = gc.Client;
            transaction.Group = gc.Group;
            transaction.Id= Guid.NewGuid().ToString();
            if (gc.R == "Забанен")
            {
                return RedirectToAction("BannedInGroup", new { name = transaction.Group.Name });
            }
            if (ModelState.IsValid)
            {
                // ниже происходит автоматически если владелец.Иначе транзакция ждет подтверждения
                // по данному id группы в которой происходит транзакция нужно в бд найти эту группу и изменить в ней поле budget
               
                if (gc.R == "Создатель")
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
                        _db.Entry(gc.Client).State = EntityState.Modified;
                        _db.Transactions.Add(transaction);
                    }
                }
                else
                {
                    if (transaction.OperationType == 1 && transaction.Amount <= gc.Group.Budget)
                    {
                        transaction.Status = false;
                        transaction.YesPercent = ((float)gc.P / _db.Groups.Include(g => g.Clients).First(rp => rp.Id == transaction.GroupId).Clients.Count()) * 100.0f;
                        transaction.NoPercent = 0;
                        var vote = new Vote
                        {
                            ClientId = transaction.ClientId,
                            Client = transaction.Client,
                            TransactionId = transaction.Id,
                            Transaction = transaction,
                            V = true,
                            P = _db.Role_Powers.First(rp => rp.ClientId == transaction.ClientId && rp.GroupId == transaction.GroupId).P
                        };
                        _db.Votes.Add(vote);
                        gc.Group.Transactions.Add(transaction);
                        _db.Transactions.Add(transaction);
                    }
                    //перевод с счета группы на счет пользователя
                    else if (transaction.OperationType == 2 && transaction.Amount <= gc.Group.Budget && gc.Client.PersBudget + transaction.Amount < 2147483647)
                    {
                        transaction.Status = false;
                        transaction.YesPercent = ((float)gc.P / _db.Groups.Include(g => g.Clients).First(rp => rp.Id == transaction.GroupId).Clients.Count()) * 100.0f;
                        transaction.NoPercent = 0.0f;
                        var vote = new Vote
                        {
                            ClientId = transaction.ClientId,
                            Client = transaction.Client,
                            TransactionId = transaction.Id,
                            Transaction = transaction,
                            V = true,
                            P = _db.Role_Powers.First(rp => rp.ClientId == transaction.ClientId && rp.GroupId == transaction.GroupId).P
                        };
                        _db.Votes.Add(vote);
                        gc.Group.Transactions.Add(transaction);
                        _db.Transactions.Add(transaction);
                    }
                    else if (transaction.OperationType == 3 && transaction.Amount <= gc.Client.PersBudget)
                    {
                        transaction.StartTime = DateTime.Now;
                        transaction.Status = true;
                        gc.Group.Budget += transaction.Amount;
                        gc.Group.Transactions.Add(transaction);                        
                        _db.Entry(gc.Group).State = EntityState.Modified;
                        gc.Client.PersBudget -= transaction.Amount;
                        _db.Entry(gc.Client).State = EntityState.Modified;
                        _db.Transactions.Add(transaction);

                    }
                }
                await _db.SaveChangesAsync();
                return RedirectToAction("OpenGroup", "Groups", new { name = transaction.Group.Name });

            }
            return View(transaction);
        }

        public async Task<IActionResult> VoteYes(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var transaction = await _db.Transactions
                .Include(t => t.Client)
                .Include(t => t.Votes)
                .Include(t => t.Group)
                .ThenInclude(g=>g.Clients)
                .ThenInclude(rp=>rp.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var client = await _db.Clients.FindAsync(currentUserID);
            if (_db.Groups.First(g=>g.Id==transaction.GroupId).Clients.FirstOrDefault(rp => rp.ClientId == currentUserID && rp.R == "Забанен") != null)
            {
                return RedirectToAction("BannedInGroup", new { name = transaction.Group.Name });
            }
            if (transaction.Status==false) {
                var vote = new Vote
                {
                    ClientId = currentUserID,
                    Client = client,
                    TransactionId = transaction.Id,
                    Transaction = transaction,
                    V = true,
                    P = _db.Role_Powers.First(rp => rp.ClientId == currentUserID && rp.GroupId == transaction.GroupId).P
                };
                _db.Votes.Add(vote);
                int YesCounter = _db.Votes.Where(v => v.TransactionId == id && v.V == true).Sum(v => v.P);
                int NoCounter = _db.Votes.Where(v => v.TransactionId == id && v.V == false).Sum(v => v.P);
                YesCounter += vote.P;
                transaction.YesPercent = ((float)YesCounter / transaction.Group.Clients.Count()) * 100.0f;
                transaction.NoPercent = ((float)NoCounter / transaction.Group.Clients.Count()) * 100.0f;
                if (transaction.YesPercent > 50 || (transaction.YesPercent == 50 && transaction.NoPercent == 50 && transaction.Group.CloseCall == true))
                {
                    //перевод с счета группы на причину снятия
                    if (transaction.OperationType == 1 && transaction.Amount <= transaction.Group.Budget)
                    {
                        transaction.StartTime = DateTime.Now;
                        transaction.Status = true;
                        transaction.Group.Budget -= transaction.Amount;
                        _db.Entry(transaction.Group).State = EntityState.Modified;
                        _db.Entry(transaction).State = EntityState.Modified;

                    }
                    //перевод с счета группы на счет пользователя
                    else if (transaction.OperationType == 2 && transaction.Amount <= transaction.Group.Budget)
                    {
                        transaction.StartTime = DateTime.Now;
                        transaction.Status = true;
                        transaction.Group.Budget -= transaction.Amount;                        
                        _db.Entry(transaction.Group).State = EntityState.Modified;
                        transaction.Client.PersBudget += transaction.Amount;
                        _db.Entry(transaction.Client).State = EntityState.Modified;                       
                    }                   
                }
                else if (transaction.NoPercent > 50 || (transaction.YesPercent == 50 && transaction.NoPercent == 50 && transaction.Group.CloseCall == false))
                {
                    _db.Votes.RemoveRange(transaction.Votes);
                    _db.Transactions.Remove(transaction);
                }
                await _db.SaveChangesAsync();
            }
                return RedirectToAction("OpenGroup", "Groups", new { name = transaction.Group.Name });
            
        }

        public async Task<IActionResult> VoteNo(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var transaction = await _db.Transactions
                .Include(t => t.Client)
                .Include(t => t.Votes)
                .Include(t => t.Group)                
                .ThenInclude(g => g.Clients)
                .ThenInclude(rp => rp.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transaction == null)
            {
                return NotFound();
            }
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var client = await _db.Clients.FindAsync(currentUserID);
            if (_db.Groups.First(g => g.Id == transaction.GroupId).Clients.FirstOrDefault(rp => rp.ClientId == currentUserID && rp.R == "Забанен") != null)
            {
                return RedirectToAction("BannedInGroup", new { name = transaction.Group.Name });
            }
            if (transaction.Status == false)
            {
                var vote = new Vote
                {
                    ClientId = currentUserID,
                    Client = client,
                    TransactionId = transaction.Id,
                    Transaction = transaction,
                    V = false,
                    P = _db.Role_Powers.First(rp => rp.ClientId == currentUserID && rp.GroupId == transaction.GroupId).P
                };
                _db.Votes.Add(vote);
                int YesCounter = _db.Votes.Where(v => v.TransactionId == id && v.V == true).Sum(v => v.P);
                int NoCounter = _db.Votes.Where(v => v.TransactionId == id && v.V == false).Sum(v => v.P);
                NoCounter += vote.P;
                transaction.YesPercent = ((float)YesCounter / transaction.Group.Clients.Count()) * 100.0f;
                transaction.NoPercent = ((float)NoCounter / transaction.Group.Clients.Count()) * 100.0f;
                if (transaction.NoPercent > 50 || (transaction.YesPercent == 50 && transaction.NoPercent == 50 && transaction.Group.CloseCall == false))
                {                   
                    _db.Votes.RemoveRange(transaction.Votes);                    
                    _db.Transactions.Remove(transaction);
                }
                else if (transaction.YesPercent > 50 || (transaction.YesPercent == 50 && transaction.NoPercent == 50 && transaction.Group.CloseCall == true))
                {
                    //перевод с счета группы на причину снятия
                    if (transaction.OperationType == 1 && transaction.Amount <= transaction.Group.Budget)
                    {
                        transaction.StartTime = DateTime.Now;
                        transaction.Status = true;
                        transaction.Group.Budget -= transaction.Amount;
                        _db.Entry(transaction.Group).State = EntityState.Modified;
                        _db.Entry(transaction).State = EntityState.Modified;

                    }
                    //перевод с счета группы на счет пользователя
                    else if (transaction.OperationType == 2 && transaction.Amount <= transaction.Group.Budget)
                    {
                        transaction.StartTime = DateTime.Now;
                        transaction.Status = true;
                        transaction.Group.Budget -= transaction.Amount;
                        _db.Entry(transaction.Group).State = EntityState.Modified;
                        transaction.Client.PersBudget += transaction.Amount;
                        _db.Entry(transaction.Client).State = EntityState.Modified;
                    }
                }
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("OpenGroup", "Groups", new { name = transaction.Group.Name });
        }
        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(string id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _db.Transactions
                .Include(t => t.Client)
                .Include(t => t.Group)
                .FirstOrDefaultAsync(m => m.Id == id);
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (_db.Groups.First(g => g.Id == transaction.GroupId).Clients.FirstOrDefault(rp => rp.ClientId == currentUserID && rp.R == "Забанен") != null)
            {
                return RedirectToAction("BannedInGroup", new { name = transaction.Group.Name });
            }
            ViewBag.Name = transaction.Group.Name;
            if (transaction == null)
            {
                return NotFound();
            }
            if (transaction.Status == true)
            {
                return RedirectToAction("OpenGroup", "Groups", new { name = transaction.Group.Name });
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var transaction = await _db.Transactions
                .Include(t => t.Client)
                .Include(t => t.Votes)
                .Include(t => t.Group)
                .ThenInclude(g => g.Clients)
                .ThenInclude(rp => rp.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            var currentUserID = this.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (_db.Groups.First(g => g.Id == transaction.GroupId).Clients.FirstOrDefault(rp => rp.ClientId == currentUserID && rp.R == "Забанен") != null)
            {
                return RedirectToAction("BannedInGroup", new { name = transaction.Group.Name });
            }
            if (transaction.Status == true)
            {
                return RedirectToAction("OpenGroup", "Groups", new { name = transaction.Group.Name });
            }
            var name = transaction.Group.Name;
            _db.Votes.RemoveRange(transaction.Votes);
            _db.Transactions.Remove(transaction);
            await _db.SaveChangesAsync();
            return RedirectToAction("OpenGroup", "Groups", new { name = transaction.Group.Name });
        }
     
    }
}
