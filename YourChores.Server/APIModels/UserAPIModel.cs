using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YourChores.Server.APIModels
{
    /// <summary>
    /// API Model to return the user information
    /// </summary>
    public class UserAPIModel
    {
        /// <summary>
        /// The response model
        /// </summary>
        public class Response
        {
            /// <summary>
            /// The id of the user
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// THe first name of the user
            /// </summary>
            public string FirstName { get; set; }

            /// <summary>
            /// The last name of the user
            /// </summary>
            public string LastName { get; set; }

            /// <summary>
            /// The user name of the user
            /// </summary>
            public string UserName { get; set; }

            /// <summary>
            /// The email of the user
            /// </summary>
            public string Email { get; set; }
        }
    }
}
