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
    private const string Me = "me";

    [Function(nameof(Profile))]
    [OpenApiOperation(nameof(Profile), Auth, Description = "Profile information")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Unauthorized)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, MediaTypeNames.Application.Json, typeof(MeResponse))]
    public Task<Results<Ok<MeResponse>, UnauthorizedHttpResult>> Profile(
        [HttpTrigger(AuthorizationLevel.Anonymous,"get", Route = Me)] HttpRequestData requestData,
        CancellationToken ct)
    {
        if (context.Principal is null)
        {
            return Task.FromResult<Results<Ok<MeResponse>, UnauthorizedHttpResult>>(TypedResults.Unauthorized());
        }

        return Task.FromResult<Results<Ok<MeResponse>, UnauthorizedHttpResult>>(
            TypedResults.Ok(
                new MeResponse(
                    context.Principal.FindFirst(IdentityDefaults.Claims.UserName)?.Value ?? string.Empty,
                    context.Principal.FindFirst(IdentityDefaults.Claims.UserEmail)?.Value ?? string.Empty,
                    context.Principal.FindFirst(IdentityDefaults.Claims.UserPhone)?.Value ?? string.Empty,
                    context.Principal.FindFirst(IdentityDefaults.Claims.UserPhoneConfirmed)?.Value ?? string.Empty,
                    context.Principal.FindFirst(IdentityDefaults.Claims.UserRoles)?.Value.Split(IdentityDefaults.Claims.Delemiter) ?? [])));
    }
}