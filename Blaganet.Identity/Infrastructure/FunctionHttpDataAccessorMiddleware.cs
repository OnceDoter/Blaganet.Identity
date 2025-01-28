using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Blaganet.Identity.Infrastructure;

public class FunctionHttpDataAccessorMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var accessor = context.InstanceServices.GetRequiredService<HttpDataAccessor>();
        accessor.Request = await context.GetHttpRequestDataAsync();
        accessor.Response = accessor.Request?.CreateResponse();
        await next(context);
        
        if (!context.FunctionDefinition.Name.Contains("Swagger", StringComparison.OrdinalIgnoreCase))
        {
            context.GetInvocationResult().Value = accessor.Response;
        }
    }
}

internal sealed class HttpDataAccessor
{
    public HttpRequestData? Request { get; set; }
    public HttpResponseData? Response { get; set; }
}