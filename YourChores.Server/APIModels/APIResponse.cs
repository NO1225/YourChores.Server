using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YourChores.Server.APIModels
{
    public class APIResponse<T>
        where T: new()
    {
        public bool Success => Errors.Count==0 ;

        public List<string> Errors { get; private set; }

        public T Response { get; set; }

        public APIResponse()
        {
            Errors = new List<string>();
            
        }
    }
}
