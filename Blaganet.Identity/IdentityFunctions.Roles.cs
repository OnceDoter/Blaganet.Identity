using System.Net;
using System.Net.Mime;
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
    [OpenApiParameter(IdentityDefaults.HeaderName, In = ParameterLocation.Header, Required = true, Description = "Access token for authorization")]
    [OpenApiParameter(nameof(identifier), In = ParameterLocation.Path, Required = true, Type = typeof(string))]
    [OpenApiRequestBody(MediaTypeNames.Application.Json, typeof(string[]))]
    [OpenApiResponseWithBody(HttpStatusCode.Created, MediaTypeNames.Application.Json, typeof(string[]))]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized)]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest)]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound)]
    public async Task<Results<Created<string[]>, UnauthorizedHttpResult, BadRequest, NotFound>> Assign(
        [HttpTrigger(AuthorizationLevel.Anonymous,"post", Route = Roles)] HttpRequestData requestData,
        string identifier,
        [FromBody] string[] roles,
        CancellationToken ct)
    {
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
}