using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace YourChores.Data.Models
{
    public class BaseModel
    {
        [Key]
        public string Id { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
