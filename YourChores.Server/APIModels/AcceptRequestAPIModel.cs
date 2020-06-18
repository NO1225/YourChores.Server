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
    /// API Model for the Kicking a member outside the room
    /// </summary>
    public class AcceptRequestAPIModel
    {
        /// <summary>
        /// The request model
        /// </summary>
        public class Request
        {
            /// <summary>
            /// The id of the room we are managing
            /// </summary>
            [Required]
            public int RoomId { get; set; }

            /// <summary>
            /// The id of the join rquest
            /// </summary>
            [Required]
            public int JoinRequestId { get; set; }
        }

    }
}
