using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace YourChores.Server.APIModels
{
    /// <summary>
    /// API model to send join request to a room
    /// </summary>
    public class JoinRoomRequestAPIModel
    {
        /// <summary>
        /// The request model
        /// </summary>
        public class Request
        {
            /// <summary>
            /// The id of the room
            /// </summary>
            /// <example>1</example>
            [Required]
            public int RoomId { get; set; }

        }


    }
}
