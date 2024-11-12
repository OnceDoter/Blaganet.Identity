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
            .AddSignInManager<IdentityFunctions.SignInManager>();
        
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
}