﻿using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Tasks.Middleware
{
    public class EnableMultipleStreamReadMiddleware : IMiddleware
    {
          
        // DEMO: Enable multi-stream read

        /// <summary>
        /// Implements the InvokeAsync method to support multiple reads from the Response.Body
        /// </summary>
        /// <param name="context">The HttpContext</param>
        /// <param name="next">The request delegate</param>
        /// <returns>A Task</returns>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Allow reading from the ResponseBody multiple times
            context.Request.EnableBuffering();

            // Call the next middleware
            await next(context);
        }
    }
}

