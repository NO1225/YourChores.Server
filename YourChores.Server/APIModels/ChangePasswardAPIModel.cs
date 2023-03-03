using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace YourChores.Server.APIModels
{
    /// <summary>
    /// The api model for changin the passward
    /// </summary>
    public class ChangePasswardAPIModel
    {
        /// <summary>
        /// The request model
        /// </summary>
        public class Request
        {
            /// <summary>
            /// The old passward
            /// </summary>
            [Required]
            public string OldPassward { get; set; }

            /// <summary>
            /// The new passward
            /// </summary>
            [Required]
            public string NewPassward { get; set; }
        }


    }
}
