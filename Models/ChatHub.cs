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
        public async Task Send(Message message/*,string text*/)
        {
            message.Name = Context.User.Identity.Name.Substring(0, Context.User.Identity.Name.LastIndexOf("@"));
            /*Message message = new Message { Text = text};*/
            //если клиент состоит в группе и его роль не забанен
            if (_db.Clients.AsNoTracking().Include(c => c.MyGroups).ThenInclude(rp => rp.Group)
                .FirstOrDefault(c => c.Login == Context.User.Identity.Name).MyGroups
                .FirstOrDefault(rp => rp.Group.Name == message.GroupName).R != "Забанен" || 
                _db.Clients.AsNoTracking().Include(c => c.MyGroups).ThenInclude(rp => rp.Group)
                .FirstOrDefault(c => c.Login == Context.User.Identity.Name).MyGroups
                .FirstOrDefault(rp => rp.Group.Name == message.GroupName) != null )
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, message.GroupName);
                //объявляем сообщение нового дня              
                if (message.Time.Day - _db.Groups.Include(g => g.Messages).FirstOrDefault(g => g.Name == message.GroupName).Messages.Last().Time.Day >= 1)
                { await Clients.Group(message.GroupName).SendAsync("Receive", message.Name, message.Text, message.Time.ToString("HH:mm:ss"), message.Time.ToString("ddd , dd/MM/yy")); }
                else { await Clients.Group(message.GroupName).SendAsync("Receive", message.Name, message.Text, message.Time.ToString("HH:mm:ss")); }
                message.ClientId = _db.Clients.AsNoTracking().FirstOrDefault(c => c.Login == Context.User.Identity.Name).Id;
                message.GroupId =  _db.Groups.AsNoTracking().FirstOrDefault(g => g.Name == message.GroupName).Id;
                await _db.Messages.AddAsync(message);
                await _db.SaveChangesAsync();

                /*
                 
                 */
            }
        }
        /*
           public async Task Send(string message, string userName)
           {

               await Clients.All.SendAsync("Send", message + $" {DateTime.Now.ToShortTimeString()}", userName);
           }*/
    }
}
