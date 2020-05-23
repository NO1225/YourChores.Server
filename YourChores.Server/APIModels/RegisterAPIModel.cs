using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourChores.Server.APIModels
{
    public class RegisterAPIModel
    {
        public class Request
        {
            [Required]
            public string UserName { get; set; }

            [EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Passward { get; set; }
        }

        public class Response 
        {
            public string UserName { get; set; }

            public string Email { get; set; }

            public IEnumerable<IdentityError> Errors { get; internal set; }
        }
    }
}
