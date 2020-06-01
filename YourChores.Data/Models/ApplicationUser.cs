using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace YourChores.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(50)]
        public string Firstname { get; set; }

        [MaxLength(50)]
        public string Lastname { get; set; }

        public IList<RoomUser> RoomUsers { get; set; }

        public IList<RoomJoinRequest> RoomJoinRequests { get; set; }
    }
}
