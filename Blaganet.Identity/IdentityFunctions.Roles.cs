using System.Net;
using System.Net.Mime;
using Blaganet.Identity.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;

namespace Blaganet.Identity;

internal partial class IdentityFunctions
{
    private const string Roles = "users/{identifier}/roles";

    [Function(nameof(Assign))]
    [OpenApiOperation(nameof(Assign), Auth, Description = "Assign roles to users")]
    [OpenApiParameter(nameof(identifier), In = ParameterLocation.Path, Required = true, Type = typeof(string))]
    [OpenApiRequestBody(MediaTypeNames.Application.Json, typeof(string[]))]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, MediaTypeNames.Application.Json, typeof(ValidationProblem))]
    public async Task<Results<Created<string[]>, UnauthorizedHttpResult, BadRequest, NotFound>> Assign(
        [HttpTrigger(AuthorizationLevel.Anonymous,"post", Route = Roles)] HttpRequestData requestData,
        string identifier,
        [FromBody] string[] roles,
        CancellationToken ct)
    {
        var checkResult = await CheckPerformerRole(IdentityDefaults.Roles.SystemAdministrator);
        if (checkResult.Result is UnauthorizedHttpResult unauthorized)
        {
            return unauthorized;
        }
        
        if (checkResult.Result is BadRequest badRequest)
        {
            return badRequest;
        }

        var user = await userManager.FindByIdAsync(identifier)
                   ?? await userManager.FindByNameAsync(identifier)
                   ?? await userManager.FindByEmailAsync(identifier);
        if (user is null)
        {
            return TypedResults.NotFound();
        }
        
        await userManager.AddToRolesAsync(user, roles);
        return TypedResults.Created(Roles, (await userManager.GetRolesAsync(user)).ToArray());
    }

    private Task<Results<UnauthorizedHttpResult, BadRequest, Ok>> CheckPerformerRole(string role)
    {
        if (context.Principal is null)
        {
            return Task.FromResult<Results<UnauthorizedHttpResult, BadRequest, Ok>>(TypedResults.Unauthorized());
        }

        var performerRoles =  context.Principal.FindFirst(IdentityDefaults.Claims.UserRoles)?.Value
            .Split(IdentityDefaults.Claims.Delemiter) ?? [];
        if (!performerRoles.Contains(role))
        {
            return Task.FromResult<Results<UnauthorizedHttpResult, BadRequest, Ok>>(TypedResults.Unauthorized());
        }
        
        return Task.FromResult<Results<UnauthorizedHttpResult, BadRequest, Ok>>(TypedResults.Ok());
    }
}