﻿using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourChores.Server.APIModels
{
    /// <summary>
    /// API Model for the login
    /// </summary>
    public class LoginAPIModel
    {
        /// <summary>
        /// The request model
        /// </summary>
        public class Request
        {
            /// <summary>
            /// You can either use your username or email
            /// </summary>
            /// <example>qwer</example>
            [Required]
            public string UserNameOrEmail { get; set; }

            /// <summary>
            /// The passward for loggin in
            /// </summary>
            /// <example>123123123</example>
            [Required]
            public string Passward { get; set; }
        }

        /// <summary>
        /// The response model
        /// </summary>
        public class Response 
        {
            /// <summary>
            /// The authorization token
            /// </summary>
            public string Token { get; set; }
        }
    }
}
