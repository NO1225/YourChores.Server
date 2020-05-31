using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using YourChores.Data.Enums;

namespace YourChores.Data.Models
{
    public class ToDoItem : BaseModel
    {
        [Required]
        [MaxLength(700)]
        public string Description { get; set; }

        [Required]
        public bool Done { get; set; } = false;

        [Required]
        public Urgency Urgency { get; set; }

        public ApplicationUser Doer { get; set; }

        public DateTime? DoingTime { get; set; }

        [Required]
        public Room Room { get; set; }
    }
}
