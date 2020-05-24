using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourChores.Server.APIModels
{
    public class LoginAPIModel
    {
        public class Request
        {
            [Required]
            public string UserNameOrEmail { get; set; }

            [Required]
            public string Passward { get; set; }
        }

        public class Response 
        {
            public string Token { get; set; }
        }
    }
}
