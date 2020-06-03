using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace YourChores.Server.APIModels
{
    public class InviteUserRequestAPIModel
    {
        public class Request
        {
            [Required]
            public int RoomId { get; set; }


            [Required]
            public string UserId { get; set; }
        }


    }
}
