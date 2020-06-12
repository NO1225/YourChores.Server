using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourChores.Server.APIModels
{
    /// <summary>
    /// API model for creating a room
    /// </summary>
    public class UpdateRoomAPIModel
    {
        /// <summary>
        /// The request model
        /// </summary>
        public class Request
        {      
            /// <summary>
            /// The id of the room
            /// </summary>
            [Required]
            public int RoomId { get; set; }

            /// <summary>
            /// Giving the ability to room members to share chores
            /// </summary>
            [Required]
            public bool AllowMembersToPost { get; set; }
        }

        /// <summary>
        /// The response model
        /// </summary>
        public class Response 
        {
            /// <summary>
            /// The created room name
            /// </summary>
            public string RoomName { get; set; }

            /// <summary>
            /// if the created room allow members to post
            /// </summary>
            public bool AllowMembersToPost { get; set; }
        }
    }
}
