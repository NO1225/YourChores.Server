using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YourChores.Server.APIModels
{
    public class CreateRoomAPIModel
    {
        public class Request
        {
            [Required]
            [MaxLength(50)]
            public string RoomName { get; set; }

            [Required]
            public bool AllowMembersToPost { get; set; }
        }

        public class Response 
        {

            public string RoomName { get; set; }

            public bool AllowMembersToPost { get; set; }
        }
    }
}
