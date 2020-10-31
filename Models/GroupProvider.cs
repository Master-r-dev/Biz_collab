using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Biz_collab.Models
{
    public class GroupProvider : IGroup
    {
        
        private List<Group> groups;
        public Group GetOrAddGroup(string groupName , int Budget , bool Type/*, List<Client> Clients, int AmountClients,int EntryFee,int MinPlus,int MinMinus*/)
        {
            if (groups.Exists(group => group.Name == groupName))
            {
                return groups.First();
            }
            else
            {
                Group group = new Group() { Name = groupName, Id = Guid.NewGuid().ToString(), Budget= Budget, Type= Type/*,Clients=Clients,AmountClients=AmountClients,EntryFee=EntryFee, MinPlus= MinPlus,MinMinus=MinMinus*/ };
                groups.Add(group);
                return group;
            }
        }


        public GroupProvider()
        {
            groups = new List<Group>();
        }
    }
}
