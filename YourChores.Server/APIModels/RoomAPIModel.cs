using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourChores.Data.Enums;
using YourChores.Data.Models;

namespace YourChores.Server.APIModels
{
    /// <summary>
    /// API Model for the rooms end point
    /// </summary>
    public class RoomAPIModel
    {
        /// <summary>
        /// The response model
        /// </summary>
        public class Response
        {
            /// <summary>
            /// The name of the requested room
            /// </summary>
            public string RoomName { get; set; }

            /// <summary>
            /// If the requested room allow members to post
            /// </summary>
            public bool AllowMembersToPost { get; set; }

            /// <summary>
            /// Number of pending chores in this room
            /// </summary>
            public int NumberOfPendingChores { get; set; }

            /// <summary>
            /// The highest urgency chore from the pending chores in this room
            /// </summary>
            public Urgency HighestUrgency { get; set; }
        }

        /// <summary>
        /// The detailed response model
        /// </summary>
        public class DetailedResponse:Response
        {
            /// <summary>
            /// The members of this room
            /// </summary>
            public List<RoomMember> RoomMembers { get; set; }

            /// <summary>
            /// The chores of this room
            /// </summary>
            public List<Chore> Chores { get; set; }
        }

        /// <summary>
        /// Room member response model
        /// </summary>
        public class RoomMember
        {
            /// <summary>
            /// The first name of the member
            /// </summary>
            public string FirstName { get; set; }

            /// <summary>
            /// The last name of the member
            /// </summary>
            public string LastName { get; set; }
        }

        /// <summary>
        /// Chore response model
        /// </summary>
        public class Chore
        {
            /// <summary>
            /// Chore description
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// If the chore is done
            /// </summary>
            public bool Done { get; set; } 

            /// <summary>
            /// The urgency of the chore (1:low, 2:medium, 3:high)
            /// </summary>
            public Urgency Urgency { get; set; }

        }
    }
}
