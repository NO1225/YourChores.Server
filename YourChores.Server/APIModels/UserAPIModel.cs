using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YourChores.Server.APIModels
{
    public class UserAPIModel
    {
        public class Response
        {

            public string Id { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string UserName { get; set; }

            public string Email { get; set; }
        }
    }
}
