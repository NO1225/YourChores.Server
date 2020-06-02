using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YourChores.Server.APIModels
{
    public class RoomAPIModel
    {
        public class Response
        {

            public string RoomName { get; set; }

            public bool AllowMembersToPost { get; set; }
        }

        public class DetailedResponse:Response
        {
            public List<RoomMember> RoomMembers { get; set; }
        }

        public class RoomMember
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

        }
    }
}
