using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourChores.Data.Enums;
using YourChores.Data.Models;

namespace YourChores.Server.APIModels
{
    public class RoomAPIModel
    {
        public class Response
        {

            public string RoomName { get; set; }

            public bool AllowMembersToPost { get; set; }

            public int NumberOfPendingChores { get; set; }

            public Urgency HighestUrgency { get; set; }
        }

        public class DetailedResponse:Response
        {
            public List<RoomMember> RoomMembers { get; set; }

            public List<Chore> Chores { get; set; }
        }

        public class RoomMember
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }

        public class Chore
        {

            public string Description { get; set; }

            public bool Done { get; set; } 

            public Urgency Urgency { get; set; }

        }
    }
}
