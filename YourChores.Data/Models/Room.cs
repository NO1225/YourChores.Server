using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace YourChores.Data.Models
{
    public class Room : BaseModel
    {
        [Required]
        [MaxLength(50)]
        public string RoomName { get; set; }

        [Required]
        public bool AllowMembersToPost { get; set; } 

        public IList<RoomUser> RoomUsers { get; set; }

        public IList<RoomJoinRequest> RoomJoinRequests { get; set; }

        public IList<ToDoItem> ToDoItems { get; set; }
    }
}
