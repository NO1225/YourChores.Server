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
    /// API Model for the declining an invitation
    /// </summary>
    public class DeclineInvitationAPIModel
    {
        /// <summary>
        /// The request model
        /// </summary>
        public class Request
        {

            /// <summary>
            /// The id of the join rquest
            /// </summary>
            [Required]
            public int JoinRequestId { get; set; }
        }

    }
}
