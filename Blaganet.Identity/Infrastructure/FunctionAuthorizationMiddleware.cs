using System.Collections.Frozen;
using System.Net;
using Blaganet.Identity.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Blaganet.Identity.Infrastructure;

public class FunctionAuthorizationMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var accessor = context.InstanceServices.GetRequiredService<HttpDataAccessor>();
        var authentication = context.InstanceServices.GetRequiredService<AuthenticationContext>();
        var functionOptions = context.InstanceServices.GetRequiredService<FrozenDictionary<string, FunctionOptions>>();

        if (context.FunctionDefinition.Name.Contains("Swagger", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        if (functionOptions.TryGetValue(context.FunctionDefinition.Name, out var option) && !option.Anonymous)
        {
            if (authentication.Principal?.Identity?.IsAuthenticated == true)
            {
                if (option.Requirements.Length == 0)
                {
                    await next(context);
                    return;
                }
                
                if (option.Requirements.Length != 0)
                {
                    var claims = authentication.Principal
                        .FindFirst(IdentityDefaults.Claims.UserRoles)?.Value
                        .Split(IdentityDefaults.Claims.Delemiter)
                        .Union(authentication.Principal.Claims.Select(claim => claim.Value))
                        ?? [];
                    
                    if (!option.Requirements.Except(claims, StringComparer.OrdinalIgnoreCase).Any())
                    {
                        await next(context);
                        return;
                    }
                    
                    if (accessor.Response is not null)
                    {
                        await accessor.Response.WriteAsJsonAsync(new { }, HttpStatusCode.NotFound);
                    }
                }
            }
            else
            {
                if (accessor.Response is not null)
                {
                    await accessor.Response.WriteAsJsonAsync(new { }, HttpStatusCode.Unauthorized);
                }
            }
        }
        else
        {
            await next(context);
        }
    }
}