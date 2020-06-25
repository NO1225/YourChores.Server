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
    public class RoomJoinRequestAPIModel
    {
        /// <summary>
        /// The response model
        /// </summary>
        public class Response
        {
            /// <summary>
            /// The id of the join request
            /// </summary>
            public int JoinRequestId { get; set; }

            /// <summary>
            /// The id of the member
            /// </summary>
            public string UserId { get; set; }

            /// <summary>
            /// The first name of the member
            /// </summary>
            public string FirstName { get; set; }

            /// <summary>
            /// The last name of the member
            /// </summary>
            public string LastName { get; set; }

            /// <summary>
            /// The id of the room
            /// </summary>
            public int RoomId { get; set; }

            /// <summary>
            /// The name of the room
            /// </summary>
            public string RoomName { get; set; }

            /// <summary>
            /// The type of this join request
            /// </summary>
            public JoinRequestType JoinRequestType { get; set; }
        }

    }
}
