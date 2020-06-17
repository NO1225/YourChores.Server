using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourChores.Server.APIModels
{
    /// <summary>
    /// API model for creating a room
    /// </summary>
    public class SearchMemberAPIModel
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
            /// The user name of the member we are trying to find
            /// </summary>
            [Required]
            public string userName { get; set; }
        }

        /// <summary>
        /// The response model
        /// </summary>
        public class Response
        {
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
            /// The User name of the member
            /// </summary>
            public string UserName { get; set; }

            /// <summary>
            /// If this user is a member of the room
            /// </summary>
            public bool IsMember { get; set; }

            /// <summary>
            /// If this user is already invited to this room
            /// </summary>
            public bool IsInvited { get; set; }

            /// <summary>
            /// If this user has already sent a join request
            /// </summary>
            public bool IsRequestingJoin { get; set; }
        }

    }
}
