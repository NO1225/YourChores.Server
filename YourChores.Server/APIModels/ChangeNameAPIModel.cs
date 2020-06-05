using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace YourChores.Server.APIModels
{
    /// <summary>
    /// The api model for changing the first and last name of a user
    /// </summary>
    public class ChangeNameAPIModel
    {
        /// <summary>
        /// The request model
        /// </summary>
        public class Request
        {
            /// <summary>
            /// The first name
            /// </summary>
            /// <example>firstName</example>
            [MaxLength(50)]
            [Required]
            public string Firstname { get; set; }

            /// <summary>
            /// The last name
            /// </summary>
            /// <example>lastName</example>
            [MaxLength(50)]
            [Required]
            public string Lastname { get; set; }
        }


    }
}
