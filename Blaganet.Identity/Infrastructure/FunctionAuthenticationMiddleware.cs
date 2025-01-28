using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Blaganet.Identity.Infrastructure;

internal sealed class FunctionAuthenticationMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var accessor = context.InstanceServices.GetRequiredService<IHttpContextAccessor>();  
        var authentication = context.InstanceServices.GetRequiredService<AuthenticationContext>();

        if (accessor.HttpContext is not null)
        {
            authentication.Principal = accessor.HttpContext.User = 
                (await accessor.HttpContext.AuthenticateAsync()).Principal
                ?? accessor.HttpContext.User;
        }

        await next(context);
    }
}

internal sealed class AuthenticationContext
{
    public ClaimsPrincipal? Principal { get; set; }
}