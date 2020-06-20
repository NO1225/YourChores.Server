using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using YourChores.Data.Enums;

namespace YourChores.Server.APIModels
{
    /// <summary>
    /// API model for creating a chore
    /// </summary>
    public class CreateChoreAPIModel
    {
        /// <summary>
        /// The request model
        /// </summary>
        public class Request
        {
            /// <summary>
            /// The id of the room
            /// </summary>
            /// <example>Adam's Home</example>
            [Required]
            public int RoomId { get; set; }

            /// <summary>
            /// The description of the chore
            /// </summary>
            [Required]
            [MaxLength(700)]
            public string Description { get; set; }

            /// <summary>
            /// The urgency of the chore
            /// </summary>
            [Required]
            public Urgency Urgency { get; set; }
        }

        /// <summary>
        /// The response model
        /// </summary>
        public class Response 
        {
            /// <summary>
            /// The created Chore Id
            /// </summary>
            public int ChoreId { get; set; }

            /// <summary>
            /// The description of the chore
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// The urgency of the chore
            /// </summary>
            public Urgency Urgency { get; set; }
        }
    }
}
