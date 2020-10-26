using Biz_collab.Models;
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
    public class ClientController : Controller
    {
        GroupContext db = new GroupContext();
        


        public async System.Threading.Tasks.Task<ActionResult> Index()
        {

            
            var roleStore = new RoleStore<IdentityRole>(new ApplicationDbContext());
            var roleManager = new RoleManager<IdentityRole>(roleStore);

            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));


            using (var dbUser = new ApplicationDbContext())
            {

                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dbUser));               
                var user = userManager.FindByName("hhhh@gmail.com");
                

                await userManager.AddToRoleAsync(user.Id, "Admin");
                //dbUser.SaveChanges();
            }

            var userName = System.Web.HttpContext.Current.User.Identity.GetUserName();
            


            IEnumerable<Client> clients = db.Clients.Include(p => p.Groups);
            //var clients = db.Clients.Include(p=> p.Groups);
            
            

            //clients.Add(new Client { Name = "test_client", Groups = { new Group { Name = "Group_3" }, new Group { Name = "Group_4" } } });
            //db.SaveChanges();

            //ViewBag.Test = "Тест Лаба";
            ViewBag.Clients = clients;
            return View();
        }

        [Authorize]
        [HttpGet]
        public ActionResult AddClient()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult Edit(int? id)
        {
            Client client = db.Clients.Find(id);
            return View(client);
        }

        
        [HttpPost]
        public ActionResult Edit(Client client)
        {
            db.Entry(client).State = EntityState.Modified;           
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        
        public ActionResult AddClient(Client client)
        {
            db.Clients.Add(client);
            db.SaveChanges();
            return RedirectPermanent("/Client/Index");
        }

        public ActionResult Index1(Client client)
        {
            var clients = (db.Clients.Include(p => p.Groups)).Where(p=> p.Id == 2);
            return View(clients.ToList());
        }

            /*[HttpGet]
            public ActionResult AddClient()
            {
                return View();
            }

            [HttpPost]
           public ActionResult AddClient(string fname)
           {
               db.Clients.Add(new Client { Name = fname });
               db.SaveChanges();
               return RedirectPermanent("/Client/Index");
           }

           [HttpGet]
           public ActionResult AddGroups(int Id)
           {
               ViewBag.Id = Id;
               return View();
           }

           [HttpPost]
               public ActionResult AddGroups(int Id, string GroupName)
                  {
                      db.Groups.Add(new Group { ClientId = Id, Name = GroupName });
                      db.SaveChanges();
                      return RedirectPermanent("/Client/Index");
                  }*/

            /*      [HttpPost]
                  public ActionResult AddClient(Client client)
                  {
                      return View();
                  }*/
        }
}
