using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace YourChores.Server.APIModels
{
    /// <summary>
    /// The api model for changin the password
    /// </summary>
    public class ChangePasswordAPIModel
    {
        /// <summary>
        /// The request model
        /// </summary>
        public class Request
        {
            /// <summary>
            /// The old password
            /// </summary>
            [Required]
            public string OldPassword { get; set; }

            /// <summary>
            /// The new password
            /// </summary>
            [Required]
            public string NewPassword { get; set; }
        }


    }
}
