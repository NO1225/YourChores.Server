using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourChores.Server.APIModels
{
    /// <summary>
    /// API Model for the app version
    /// </summary>
    public class AppVersionAPIModel
    {
        /// <summary>
        /// The request model
        /// </summary>
        public class Request
        {
            /// <summary>
            /// The version of this download
            /// </summary>
            [Required]
            public int Version { get; set; }

            /// <summary>
            /// THe lowest allowed version to run right now
            /// </summary>
            [Required]
            public int LowestAllowedVersion { get; set; }

            /// <summary>
            /// Message to show to the user who have old version
            /// </summary>
            [Required]
            [MaxLength(700)]
            public string Message { get; set; }

            /// <summary>
            /// The download link to this version
            /// </summary>
            [Required]
            [MaxLength(700)]
            public string DownloadURL { get; set; }
        }

        /// <summary>
        /// The response model
        /// </summary>
        public class Response 
        {
            /// <summary>
            /// The version of this download
            /// </summary>
            public int Version { get; set; }

            /// <summary>
            /// THe lowest allowed version to run right now
            /// </summary>
            public int LowestAllowedVersion { get; set; }

            /// <summary>
            /// Message to show to the user who have old version
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// The download link to this version
            /// </summary>
            public string DownloadURL { get; set; }
        }
    }
}
