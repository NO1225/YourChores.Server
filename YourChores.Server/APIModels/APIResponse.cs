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
    public class APIResponse
    {
        /// <summary>
        /// If the call was successfull
        /// </summary>
        public bool Success => Errors == null || Errors.Count == 0;

        /// <summary>
        /// The errors if not successfull
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// ADding an error to the error list
        /// </summary>
        /// <param name="error"></param>
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
    public class APIResponse<T> : APIResponse
        where T : new()
    {
        /// <summary>
        /// The response data
        /// </summary>
        public T Response { get; set; }

    }
}
