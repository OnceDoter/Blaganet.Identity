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
internal sealed class FunctionTypedResultsMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        await next(context);

        var result = context.GetInvocationResult();
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (result.Value is not INestedHttpResult nestedResult)
        {
            return;
        }

        var accessor = context.InstanceServices.GetRequiredService<HttpDataAccessor>();
        if (accessor.Request is null || result.Value is null || accessor.Response is null)
        {
            return;
        }

        if (nestedResult.Result is IStatusCodeHttpResult statusCodeResult)
        {
            accessor.Response.StatusCode = (HttpStatusCode?)statusCodeResult.StatusCode ?? HttpStatusCode.OK;
        }

        switch (nestedResult.Result)
        {
            case ValidationProblem validation:
                await accessor.Response.WriteAsJsonAsync(validation.ProblemDetails, HttpStatusCode.BadRequest);
                break;
            case Ok<AuthenticationTicket> signIn:
                var provider = context.InstanceServices.GetRequiredService<IDataProtectionProvider>();
                var protector = provider.CreateProtector(IdentityConstants.ApplicationScheme);
                var ticketProtector = new TicketDataFormat(protector);
                var ticket = ticketProtector.Protect(signIn.Value!);
                accessor.Response.Cookies.Append(IdentityDefaults.CookieName, ticket);
                await accessor.Response.WriteAsJsonAsync(new { ticket }, HttpStatusCode.OK);
                break;
            case IValueHttpResult valueHttpResult:
                await accessor.Response.WriteAsJsonAsync(valueHttpResult.Value, HttpStatusCode.OK);
                break;
            default:
                await accessor.Response.WriteAsJsonAsync(nestedResult.Result, HttpStatusCode.BadRequest);
                break;
        }
    }
}