using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using System.Security.Claims;
using Blaganet.Identity.Contract;
using Microsoft.AspNetCore.Authentication;
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
    
    [Function(nameof(Me))]
    [OpenApiOperation(nameof(Me), Auth, Description = "Sing in")]
    [OpenApiRequestBody(MediaTypeNames.Application.Json, typeof(SingInRequest))]
    [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
    [OpenApiResponseWithBody(HttpStatusCode.BadRequest, MediaTypeNames.Application.Json, typeof(ValidationProblem))]
    public async Task<Results<Ok<AuthenticationTicket>, BadRequest>> SingIn(
        [HttpTrigger(AuthorizationLevel.Anonymous,"post", Route = $"{Auth}/signin")] HttpRequestData requestData,
        [FromBody] SingInRequest request,
        CancellationToken ct)
    {
        await signInManager.PasswordSignInAsync(request.Email, request.Password, true, lockoutOnFailure: true);

        return await IssueTicket(request.Email);
    }

    private async Task<Results<Ok<AuthenticationTicket>, BadRequest>> IssueTicket(string login)
    {
        var user = await userManager.FindByNameAsync(login);
        if (user is null)
        {
            return TypedResults.BadRequest();
        }

        var userPrincipal = await signInManager.CreateUserPrincipalAsync(user);
        var roles = await userManager.GetRolesAsync(user);
        
        userPrincipal.Identities.First().AddClaims([
            new Claim("amr", "pwd"),
            new Claim(IdentityDefaults.Claims.UserEmail, user.Email ?? string.Empty),
            new Claim(IdentityDefaults.Claims.UserName, user.UserName ?? string.Empty),
            new Claim(IdentityDefaults.Claims.UserPhone, user.PhoneNumber ?? string.Empty),
            new Claim(IdentityDefaults.Claims.UserPhoneConfirmed, user.PhoneNumberConfirmed.ToString()),
            new Claim(IdentityDefaults.Claims.UserRoles, string.Join(IdentityDefaults.Claims.Delemiter, roles)),
        ]);

        return TypedResults.Ok(
            new AuthenticationTicket(
                userPrincipal,
                new AuthenticationProperties { IsPersistent = true },
                IdentityConstants.ApplicationScheme));
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