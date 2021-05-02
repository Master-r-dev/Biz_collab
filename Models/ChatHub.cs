using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Biz_collab.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Biz_collab.Models
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _db;
        public ChatHub(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task Send(string Text,string Groupname ,string notify)
        {
            //если клиент состоит в группе и его роль не забанен
            var client = _db.Clients.Include(c => c.MyGroups).ThenInclude(rp => rp.Group).ThenInclude(c=>c.Clients)
                .FirstOrDefault(c => c.Login == Context.User.Identity.Name).MyGroups
                .FirstOrDefault(rp => rp.Group.Name == Groupname);
            Message message = new Message { Text = Text, Time=DateTime.Now };
            message.Name = Context.User.Identity.Name.Substring(0, Context.User.Identity.Name.LastIndexOf("@"));
            
            if (client.R != "Забанен" || client != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, Groupname);
                //объявляем сообщение нового дня              
                if (!_db.Groups.AsNoTracking().Include(g => g.Messages).FirstOrDefault(g => g.Name == Groupname).Messages.Any() || message.Time.Day - _db.Groups.AsNoTracking().Include(g => g.Messages).FirstOrDefault(g => g.Name == Groupname).Messages.Last().Time.Day >= 1)
                { await Clients.Group(Groupname).SendAsync("Receive", message.Name, message.Text, message.Time.ToString("HH:mm:ss"), message.Time.ToString("ddd , dd/MM/yy")); }
                else
                { await Clients.Group(Groupname).SendAsync("Receive", message.Name, message.Text, message.Time.ToString("HH:mm:ss"), null); }
                message.ClientId = _db.Clients.AsNoTracking().FirstOrDefault(c => c.Login == Context.User.Identity.Name).Id;
                message.GroupId = _db.Groups.AsNoTracking().FirstOrDefault(g => g.Name == Groupname).Id;
                 if (client.R == "Создатель" || client.R == "Mod" || client.R == "VIP") {
                    if (notify == "all")
                    {
                        foreach (var c in client.Group.Clients)
                        {
                            Notification callUser = new Notification
                            {
                                ClientId = c.Client.Id,
                                NotiHeader = "Вас упомянули в чате",
                                NotiBody = "В группе: " + Groupname + " вызван пользователем:   " + message.Name,
                                IsRead = false,
                                Url = "../Groups/OpenGroup?name=" + Groupname
                            };
                           await _db.Notifications.AddAsync(callUser);
                        }
                    }
                    else if (notify != "")
                    {
                        foreach (var c in client.Group.Clients)
                        {
                            if (c.Client.Login==notify) {
                                Notification callUser = new Notification
                                {
                                    ClientId = c.Client.Id,
                                    NotiHeader = "Вас упомянули в чате",
                                    NotiBody = "В группе: " + Groupname + " вызван пользователем:   " + message.Name,
                                    IsRead = false,
                                    Url = "../Groups/OpenGroup?name=" + Groupname
                                };
                              await  _db.Notifications.AddAsync(callUser);
                            }
                        }
                    }
                 }
                await _db.Messages.AddAsync(message);
                await _db.SaveChangesAsync();
            }
        }
        /*
           public async Task Send(string message, string userName)
           {

               await Clients.All.SendAsync("Send", message + $" {DateTime.Now.ToShortTimeString()}", userName);
           }*/
    }
}
