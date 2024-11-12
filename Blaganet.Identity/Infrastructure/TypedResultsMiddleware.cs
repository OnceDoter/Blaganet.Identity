using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Blaganet.Identity.Infrastructure;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class TypedResultsMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);

            var result = context.GetInvocationResult();
            if (result.Value is not INestedHttpResult nestedResult)
            {
                return;
            }
            
            var request = await context.GetHttpRequestDataAsync();
            if (request is null || result.Value is null)
            {
                return;
            }
            
            var response = nestedResult.Result switch
            {
                NotFound => request.CreateResponse(HttpStatusCode.NotFound),
                BadRequest => request.CreateResponse(HttpStatusCode.BadRequest),
                ValidationProblem => request.CreateResponse(HttpStatusCode.BadRequest),
                _ => request.CreateResponse(HttpStatusCode.OK),
            };

            switch (nestedResult.Result)
            {
                case ValidationProblem validation:
                    await response.WriteAsJsonAsync(validation.ProblemDetails, HttpStatusCode.BadRequest);
                    break;
                case Ok<AuthenticationTicket> signIn:
                    var provider = context.InstanceServices.GetRequiredService<IDataProtectionProvider>();
                    var protector = provider.CreateProtector(IdentityConstants.ApplicationScheme);
                    var ticketProtector = new TicketDataFormat(protector);
                    var ticket = ticketProtector.Protect(signIn.Value!);
                    response.Cookies.Append(IdentityDefaults.CookieName, ticket);
                    await response.WriteAsJsonAsync(new { ticket }, HttpStatusCode.OK);
                    break;
                case IValueHttpResult valueHttpResult:
                    await response.WriteAsJsonAsync(valueHttpResult.Value, HttpStatusCode.OK);
                    break;
            }

            context.GetInvocationResult().Value = response;
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