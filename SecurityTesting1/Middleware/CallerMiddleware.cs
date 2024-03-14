using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;
using SecurityTesting1.Common.Services;

namespace SecurityTesting1.Common.Middleware
{
    public class CallerMiddleware
    {
        private readonly RequestDelegate _next;

        public CallerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, CallerService callerService)
        {
            //Get caller's identity.
            string appNameOrUserId = "Anonymous";
            if (!String.IsNullOrWhiteSpace(context.User?.Identity?.Name))
            {
                appNameOrUserId = context.User.Identity.Name;
            }

            callerService.PerformedBy = appNameOrUserId;

            //Get user agent
            callerService.UserAgent = context.Request?.Headers?["User-Agent"].FirstOrDefault() ?? String.Empty;

            //Get calller's IP address
            bool isForwardedIpAddressAllowed = true;

            if (isForwardedIpAddressAllowed)
            {
                string forwardedIpAddressHeaderValue = context.Request?.Headers?["X-Forwarded-For"].FirstOrDefault() ?? String.Empty;
                IEnumerable<string> forwardedIpAddresses = forwardedIpAddressHeaderValue.Split(new char[] { ',' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                if (forwardedIpAddresses.Any())
                {
                    callerService.FromRemoteIpAddress = forwardedIpAddresses.First();
                }
                else
                {
                    callerService.FromRemoteIpAddress = context.Connection?.RemoteIpAddress?.ToString() ?? String.Empty;
                }
            }
            else
            {
                callerService.FromRemoteIpAddress = context.Connection?.RemoteIpAddress?.ToString() ?? String.Empty;
            }

            // Call the next delegate/middleware in the pipeline
            await this._next(context);
        }
    }
}
