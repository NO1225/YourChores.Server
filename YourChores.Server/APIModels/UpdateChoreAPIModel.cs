using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourChores.Server.APIModels
{
    /// <summary>
    /// API model for creating a room
    /// </summary>
    public class UpdateChoreAPIModel
    {
        /// <summary>
        /// The request model
        /// </summary>
        public class Request
        {      
            /// <summary>
            /// The id of the chore
            /// </summary>
            [Required]
            public int ChoreId { get; set; }

            /// <summary>
            /// The id of the room
            /// </summary>
            [Required]
            public int RoomId { get; set; }
        }

    }
}
