using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace Blaganet.Identity.Infrastructure;

internal sealed class FunctionHttpContextAccessorMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContextAccessor = context.InstanceServices.GetRequiredService<IHttpContextAccessor>();
        var httpDataAccessor = context.InstanceServices.GetRequiredService<HttpDataAccessor>();
        if (httpContextAccessor.HttpContext is not null && httpDataAccessor.Request is not null)
        {
            httpContextAccessor.HttpContext.RequestServices = context.InstanceServices;
            foreach (var header in httpDataAccessor.Request.Headers)
            {
                httpContextAccessor.HttpContext.Request.Headers.Append(header.Key, new StringValues(header.Value.ToArray()));
            }
        }

        await next(context);
        
        if (httpDataAccessor.Response is not null && httpContextAccessor.HttpContext is not null)
        {
            foreach (var header in httpContextAccessor.HttpContext.Response.Headers)
            {
                httpDataAccessor.Response.Headers.Add(header.Key, header.Value.ToString());
            }
        }
    }
}

internal sealed class HttpContextAccessor : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; } = new DefaultHttpContext();
}