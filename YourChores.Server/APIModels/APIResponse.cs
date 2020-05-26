﻿using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YourChores.Server.APIModels
{
    public class APIResponse<T>
        where T : new()
    {
        public bool Success => Errors == null||Errors.Count==0 ;

        public List<string> Errors { get; set; }

        public T Response { get; set; }

        public void AddError(string error)
        {
            if (Errors == null)
                Errors = new List<string>();

            Errors.Add(error);
        }

    }
}
