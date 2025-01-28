using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using Blaganet.Identity.Contract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace Blaganet.Identity;

internal partial class IdentityFunctions
{
    private const string Auth = "auth";
    
    [Function(nameof(SingUp))]
    [OpenApiOperation(nameof(SingUp), Auth, Description = "Sing up")]
    [OpenApiRequestBody(MediaTypeNames.Application.Json, typeof(SingUpRequest))]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, MediaTypeNames.Application.Json, typeof(ValidationProblem))]
    public async Task<Results<Ok, ValidationProblem>> SingUp(
        [HttpTrigger(AuthorizationLevel.Anonymous,"post", Route = $"{Auth}/signup")] HttpRequestData requestData,
        [FromBody] SingUpRequest request,
        CancellationToken ct)
    {
        var user = new IdentityUser();
        await userStore.SetUserNameAsync(user, request.Email, ct);
        await ((IUserEmailStore<IdentityUser>)userStore).SetEmailAsync(user, request.Email, ct);

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return CreateValidationProblem(result);
        }

        return TypedResults.Ok();
    }
    
    [Function(nameof(SingIn))]
    [OpenApiOperation(nameof(SingIn), Auth, Description = "Sing in")]
    [OpenApiRequestBody(MediaTypeNames.Application.Json, typeof(SingInRequest))]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, MediaTypeNames.Application.Json, typeof(ValidationProblem))]
    public async Task SingIn(
        [HttpTrigger(AuthorizationLevel.Anonymous,"post", Route = $"{Auth}/signin")] HttpRequestData requestData,
        [FromBody] SingInRequest request,
        CancellationToken ct)
    {
        await signInManager.PasswordSignInAsync(request.Email, request.Password, true, lockoutOnFailure: true);
    }
    
    private static ValidationProblem CreateValidationProblem(IdentityResult result)
    {
        Debug.Assert(!result.Succeeded);
        var errorDictionary = new Dictionary<string, string[]>(1);

        foreach (var error in result.Errors)
        {
            string[] newDescriptions;

            if (errorDictionary.TryGetValue(error.Code, out var descriptions))
            {
                newDescriptions = new string[descriptions.Length + 1];
                Array.Copy(descriptions, newDescriptions, descriptions.Length);
                newDescriptions[descriptions.Length] = error.Description;
            }
            else
            {
                newDescriptions = [error.Description];
            }

            errorDictionary[error.Code] = newDescriptions;
        }

        return TypedResults.ValidationProblem(errorDictionary);
    }
}