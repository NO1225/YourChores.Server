using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using YourChores.Data.Enums;

namespace YourChores.Data.Models
{
    public class RoomJoinRequest : BaseModel
    {
        [Required]
        public ApplicationUser User { get; set; }

        [Required]
        public JoinRequestType JoinRequestType { get; set; }

        [Required]
        public Room Room { get; set; }

        [Required]
        public bool Declined { get; set; } = false;
    }
}
