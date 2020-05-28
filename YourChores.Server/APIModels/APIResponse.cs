using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YourChores.Server.APIModels
{
    /// <summary>
    ///  Base response in case there is no data 
    /// </summary>
    public class APIResonse
    {
        public bool Success => Errors == null || Errors.Count == 0;

        public List<string> Errors { get; set; }

        public void AddError(string error)
        {
            if (Errors == null)
                Errors = new List<string>();

            Errors.Add(error);
        }
    }

    /// <summary>
    /// Base response with data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class APIResponse<T> : APIResonse
        where T : new()
    {

        public T Response { get; set; }

    }
}
