using Biz_collab.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using Biz_collab.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;


namespace Biz_collab.Controllers
{
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _db;

        public TransactionController(ApplicationDbContext db)
        {
            _db = db;
        }
      //  ApplicationDbContext db = new ApplicationDbContext();
        // GET: Transaction
        public ActionResult Index(string GroupId)
        {


            string groupId = GroupId;
            IEnumerable<Transaction> transactions = _db.Transactions;
            ViewBag.Transactions = transactions;
            return View();
        }

        // GET: Transaction/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Transaction/Create
        [HttpGet]
        public ActionResult Create(string GroupId)
        {
            ClaimsPrincipal currentUser = this.User;
            var currentUserID = currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;

            Transaction transaction = new Transaction
            {
                UserId = currentUserID,
                GroupId = GroupId
            };

            return View(transaction);
        }

        // POST: Transaction/Create
        [HttpPost]
        public ActionResult Create(Transaction transaction, int Amount, bool OperationType, string Explanation)
        {
            _db.Transactions.Add(new Transaction
            {
                UserId = transaction.UserId,
                GroupId = transaction.GroupId,
                Amount = Amount,
                OperationType = OperationType,
                Explanation = Explanation,
                StartTime = DateTime.Now
            });

            // ниже происходит автоматически если владелец.Иначе транзакция ждет подтверждения
            // по данному id группы в которой происходит транзакция нужно в бд найти эту группу и изменить в ней поле budget
            var group = (_db.Groups.Include(p => p.Clients)).Where(prop => prop.Id == transaction.GroupId).FirstOrDefault();
            if (transaction.OperationType) group.Budget += transaction.Amount;
            else group.Budget -= transaction.Amount;
            _db.Entry(group).State = EntityState.Modified;
            _db.SaveChanges();
            return RedirectPermanent("/Transaction/Index");
        }

        // GET: Transaction/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Transaction/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Transaction/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Transaction/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
