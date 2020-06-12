using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using YourChores.Data.Enums;
using YourChores.Data.Models;

namespace YourChores.Server.APIModels
{
    /// <summary>
    /// API Model for the rooms end point
    /// </summary>
    public class LeaveRoomAPIModel
    {
        /// <summary>
        /// The request model
        /// </summary>
        public class Request
        {
            /// <summary>
            /// The id of the room we are going to leave
            /// </summary>
            [Required]
            public int RoomId { get; set; }

            /// <summary>
            /// The id of the user who should replace us of we were the last owner of this room, and the room has other user
            /// </summary>
            public string AlternativeId { get; set; }
        }

    }
}
