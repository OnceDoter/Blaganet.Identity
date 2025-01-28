using System.Collections.Frozen;
using Blaganet.Identity.Adapters;
using Blaganet.Identity.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Blaganet.Identity.Infrastructure;

public static class ServiceCollectionExtensionMethods
{
    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddDefaultPolicy(
                "default",
                static builder => builder
                    .AddAuthenticationSchemes(IdentityConstants.ApplicationScheme)
                    .RequireAuthenticatedUser())
            .AddPolicy(
                IdentityDefaults.Roles.SystemAdministrator,
                static builder => builder
                    .AddAuthenticationSchemes(IdentityConstants.ApplicationScheme)
                    .RequireAuthenticatedUser()
                    .RequireRole(IdentityDefaults.Roles.SystemAdministrator));

        services
            .AddAuthentication()
            .AddCookie(IdentityConstants.ApplicationScheme);
        
        services
            .AddIdentityCore<IdentityUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddClaimsPrincipalFactory<IdentityFunctions.UserClaimsPrincipalFactory>()
            .AddSignInManager();

        services.AddSingleton<FrozenDictionary<string, FunctionOptions>>(
            _ => GetFunctionOptions().ToFrozenDictionary(option => option.Name));
        
        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSingleton<IOpenApiConfigurationOptions>(
            _ => new OpenApiConfigurationOptions
            {
                Info = new OpenApiInfo { Version = "1.0", Title = IdentityDefaults.ServiceName },
                OpenApiVersion = OpenApiVersionType.V3
            });

        return services;
    }
    
    public static IServiceCollection AddAdapters(this IServiceCollection services)
    {
        services.AddScoped<HttpContext, FunctionHttpContext>();
        return services;
    }

    private static IEnumerable<FunctionOptions> GetFunctionOptions()
    {
        yield return new FunctionOptions
        {
            Name = nameof(IdentityFunctions.SingUp),
            Anonymous = true,
        };

        yield return new FunctionOptions
        {
            Name = nameof(IdentityFunctions.SingIn),
            Anonymous = true,
        };

        yield return new FunctionOptions
        {
            Name = nameof(IdentityFunctions.Assign),
            Requirements = [IdentityDefaults.Roles.SystemAdministrator]
        };
    }
}