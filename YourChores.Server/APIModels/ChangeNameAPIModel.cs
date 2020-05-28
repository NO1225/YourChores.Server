using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace YourChores.Server.APIModels
{
    public class ChangeNameAPIModel
    {
        public class Request
        {
            [MaxLength(50)]
            [Required]
            public string Firstname { get; set; }

            [MaxLength(50)]
            [Required]
            public string Lastname { get; set; }
        }


    }
}
