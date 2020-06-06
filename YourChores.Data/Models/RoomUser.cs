using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace YourChores.Data.Models
{
    public class RoomUser : BaseModel
    {
        [Required]
        public ApplicationUser User { get; set; }

        [Required]
        public Room Room { get; set; }

        [Required]
        public bool Owner { get; set; } = false;
    }
}
