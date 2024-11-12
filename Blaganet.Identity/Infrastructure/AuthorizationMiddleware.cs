using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Blaganet.Identity.Infrastructure;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class AuthorizationMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext fContext, FunctionExecutionDelegate next)
    {
        var httpRequestData = await fContext.GetHttpRequestDataAsync();
        if (httpRequestData != null)
        {
            if (httpRequestData.Headers.TryGetValues(IdentityDefaults.HeaderName, out var authorization))
            {
                var provider = fContext.InstanceServices.GetRequiredService<IDataProtectionProvider>();
                var protector = provider.CreateProtector(IdentityConstants.ApplicationScheme);
                var ticketProtector = new TicketDataFormat(protector);
                var ticket = ticketProtector.Unprotect(authorization.SingleOrDefault());
                if (ticket is null)
                {
                    return;
                }
                
                var context = fContext.InstanceServices.GetRequiredService<AuthorizationContext>();
                context.Principal = ticket.Principal;
            }
        }
        
        await next(fContext);
    }
}

internal sealed class AuthorizationContext
{
    public ClaimsPrincipal? Principal { get; set; }
}