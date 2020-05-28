using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace YourChores.Server.APIModels
{
    public class ChangePasswardAPIModel
    {
        public class Request
        {
            [Required]
            public string OldPassward { get; set; }

            [Required]
            public string NewPassward { get; set; }
        }


    }
}
