using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using System.Threading.Tasks;

namespace Tasks.Middleware
{
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Helper to enable the multiple stream reader middleware
        /// </summary>
        /// <param name="builder">The application builder</param>
        /// <returns>An application builder</returns>
        public static IApplicationBuilder UseMultipleStreamReadMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<EnableMultipleStreamReadMiddleware>();
        }
    }
}