using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Biz_collab.Data;
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
        public async Task Send(string Text,string Groupname ,string[] notify)
        {
            //если клиент состоит в группе и его роль не забанен
            var client = _db.Groups.Include(g => g.Clients).ThenInclude(rp=>rp.Client).ThenInclude(c=>c.MutedName).FirstOrDefault(g => g.Name == Groupname).Clients
                .FirstOrDefault(rp=>rp.ClientId == Context.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Message message = new Message { Text = Text};
            message.Name = Context.User.Identity.Name.Substring(0, Context.User.Identity.Name.LastIndexOf("@"));
            
            if (client.R != "Забанен" || client != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, Groupname);
                //объявляем сообщение нового дня              
                if (!_db.Groups.AsNoTracking().Include(g => g.Messages).FirstOrDefault(g => g.Name == Groupname).Messages.Any() || message.Time.Day - _db.Groups.AsNoTracking().Include(g => g.Messages).FirstOrDefault(g => g.Name == Groupname).Messages.Last().Time.Day >= 1)
                { await Clients.Group(Groupname).SendAsync("Receive", message.Name, message.Text, message.Time.ToString("HH:mm:ss"), message.Time.ToString("ddd , dd/MM/yy")); }
                else
                { await Clients.Group(Groupname).SendAsync("Receive", message.Name, message.Text, message.Time.ToString("HH:mm:ss")); }
                message.ClientId = _db.Clients.AsNoTracking().FirstOrDefault(c => c.Login == Context.User.Identity.Name).Id;
                message.GroupId = _db.Groups.AsNoTracking().FirstOrDefault(g => g.Name == Groupname).Id;
                 if (client.R == "Creator" || client.R == "Mod" || client.R == "VIP") {
                    if (notify != null && notify.Length > 0)
                    {
                        if (notify[0] == "all")
                        {
                            foreach (var c in client.Group.Clients)
                            {
                                if (c.ClientId==client.ClientId) { continue; }
                                if (!c.Client.MutedName.Any(m=>m.Name == Groupname || m.Name == Context.User.Identity.Name)) {
                                    Notification callUser = new Notification
                                    {
                                        ClientId = c.ClientId,
                                        NotiHeader = "Упомянули Вас в чате",
                                        NotiBody = " Вызван пользователем-" + Context.User.Identity.Name + " в группе:" + Groupname  ,
                                        IsRead = false,
                                        Url = "../Groups/OpenGroup?name=" + Groupname
                                    };
                                    await _db.Notifications.AddAsync(callUser);
                                }
                            }

                        }
                        else
                        {
                            for (var i = 0; i < notify.Length; i++)
                            {

                                if (notify[i] == "Mod")
                                {
                                    foreach (var c in client.Group.Clients.Where(c => c.R == "Mod"))
                                    {
                                        if (c.ClientId == client.ClientId) { continue; }
                                        if (!c.Client.MutedName.Any(m=>m.Name == Groupname || m.Name == Context.User.Identity.Name))
                                        {
                                            Notification callUser = new Notification
                                            {
                                                ClientId = c.ClientId,
                                                NotiHeader = "Упомянули Вас в чате",
                                                NotiBody = " Вызван пользователем-" + Context.User.Identity.Name + " в группе:" + Groupname,
                                                IsRead = false,
                                                Url = "../Groups/OpenGroup?name=" + Groupname
                                            };
                                            await _db.Notifications.AddAsync(callUser);
                                        }
                                    }
                                }
                                else if (notify[i] == "VIP")
                                {
                                    foreach (var c in client.Group.Clients.Where(c => c.R == "VIP"))
                                    {
                                        if (c.ClientId == client.ClientId) { continue; }
                                        if (!c.Client.MutedName.Any(m=>m.Name == Groupname || m.Name == Context.User.Identity.Name))
                                        {
                                            Notification callUser = new Notification
                                            {
                                                ClientId = c.ClientId,
                                                NotiHeader = "Упомянули Вас в чате",
                                                NotiBody = " Вызван пользователем-" + Context.User.Identity.Name + " в группе:" + Groupname,
                                                IsRead = false,
                                                Url = "../Groups/OpenGroup?name=" + Groupname
                                            };
                                            await _db.Notifications.AddAsync(callUser);
                                        }
                                    }
                                }
                                else if (notify[i] == "User")
                                {
                                    foreach (var c in client.Group.Clients.Where(c => c.R == "User"))
                                    {
                                        if (c.ClientId == client.ClientId) { continue; }
                                        if (!c.Client.MutedName.Any(m=>m.Name == Groupname || m.Name == Context.User.Identity.Name))
                                        {
                                            Notification callUser = new Notification
                                            {
                                                ClientId = c.ClientId,
                                                NotiHeader = "Упомянули Вас в чате",
                                                NotiBody = " Вызван пользователем-" + Context.User.Identity.Name + " в группе:" + Groupname,
                                                IsRead = false,
                                                Url = "../Groups/OpenGroup?name=" + Groupname
                                            };
                                            await _db.Notifications.AddAsync(callUser);
                                        }
                                    }
                                }
                                else if (notify[i] == "Don")
                                {
                                    foreach (var c in client.Group.Clients.Where(c => c.R == "Don"))
                                    {
                                        if (c.ClientId == client.ClientId) { continue; }
                                        if (!c.Client.MutedName.Any(m=>m.Name == Groupname || m.Name == Context.User.Identity.Name))
                                        {
                                            Notification callUser = new Notification
                                            {
                                                ClientId = c.ClientId,
                                                NotiHeader = "Упомянули Вас в чате",
                                                NotiBody = " Вызван пользователем-" + Context.User.Identity.Name + " в группе:" + Groupname,
                                                IsRead = false,
                                                Url = "../Groups/OpenGroup?name=" + Groupname
                                            };
                                            await _db.Notifications.AddAsync(callUser);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var c in client.Group.Clients)
                                    {
                                        if (c.Client.Login == notify[i])
                                        {
                                            if (c.ClientId == client.ClientId) { continue; }
                                            if (!c.Client.MutedName.Any(m=>m.Name == Groupname || m.Name == Context.User.Identity.Name))
                                            {
                                                Notification callUser = new Notification
                                                {
                                                    ClientId = c.ClientId,
                                                    NotiHeader = "Упомянули Вас в чате",
                                                    NotiBody = " Вызван пользователем-" + Context.User.Identity.Name + " в группе:" + Groupname,
                                                    IsRead = false,
                                                    Url = "../Groups/OpenGroup?name=" + Groupname
                                                };
                                                await _db.Notifications.AddAsync(callUser);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                 }
                await _db.Messages.AddAsync(message);
                await _db.SaveChangesAsync();
            }
        }
    }
}
