using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace YourChores.Server.APIModels
{
    /// <summary>
    /// Api model to invite a user to join a toom
    /// </summary>
    public class InviteUserRequestAPIModel
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

            /// <summary>
            /// The id of the user
            /// </summary>
            /// <example>e4787142-cfe6-423d-a4ac-da15c58ef09e</example>
            [Required]
            public string UserId { get; set; }
        }


    }
}
