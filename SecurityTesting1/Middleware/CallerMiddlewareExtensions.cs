using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace SecurityTesting1.Common.Middleware
{
    public static class CallerMiddlewareExtensions
    {
        public static IApplicationBuilder UseCaller(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CallerMiddleware>();
        }
    }
}

