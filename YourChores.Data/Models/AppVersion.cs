using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using YourChores.Data.Enums;

namespace YourChores.Data.Models
{
    public class AppVersion : BaseModel
    {
        [Required]
        public int Version { get; set; }

        [Required]
        public int LowestAllowedVersion { get; set; }

        [Required]
        [MaxLength(700)]
        public string Message { get; set; }

        [Required]
        [MaxLength(700)]
        public string DownloadURL { get; set; }
    }
}
