using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Blaganet.Identity.Infrastructure;

public class FunctionExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            var request = await context.GetHttpRequestDataAsync();
            if (request is not null)
            {
                var response = request.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteAsJsonAsync(new
                {
                    Error = "An internal server error occurred.",
                    Details = e.Message
                }, HttpStatusCode.InternalServerError);
                
                context.GetInvocationResult().Value = response;
            }
            else
            {
                throw;
            }
        }
    }
}