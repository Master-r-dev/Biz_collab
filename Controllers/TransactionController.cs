﻿using Biz_collab.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security.Provider;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Biz_collab.Controllers
{
    public class TransactionController : Controller
    {
        GroupContext db = new GroupContext();
        // GET: Transaction
        public ActionResult Index(string GroupId)
        {
            
            string userId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            string groupId= GroupId;
            IEnumerable<Transaction> transactions = db.Transactions;
            ViewBag.Transactions = transactions;
            return View();
        }

        // GET: Transaction/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Transaction/Create
        public ActionResult Create(string userId, string GroupId)
        {
            Transaction transaction = new Transaction();
            transaction.UserId = userId;
            transaction.GroupId = GroupId;

            return View(transaction);
        }

        // POST: Transaction/Create
        [HttpPost]
        public ActionResult Create(Transaction transaction , int Amount, bool OperationType, string Explanation)
        {
            db.Transactions.Add(new Transaction { UserId= transaction.UserId, GroupId= transaction.GroupId, Amount = Amount , OperationType = OperationType , Explanation= Explanation, StartTime= DateTime.Now }) ;
            db.SaveChanges();
            return RedirectPermanent("/Transaction/Index");
        }

        // GET: Transaction/Edit/5
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