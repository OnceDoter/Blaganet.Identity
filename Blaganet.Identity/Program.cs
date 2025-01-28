using Blaganet.Identity;
using Blaganet.Identity.Configuration;
using Blaganet.Identity.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using HttpContextAccessor = Blaganet.Identity.Infrastructure.HttpContextAccessor;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(
        configure: static (_, builder) =>
        {
            builder.UseMiddleware<FunctionExceptionHandlingMiddleware>();
            builder.UseMiddleware<FunctionHttpDataAccessorMiddleware>();
            builder.UseMiddleware<FunctionHttpContextAccessorMiddleware>();
            builder.UseMiddleware<FunctionAuthenticationMiddleware>();
            builder.UseMiddleware<FunctionAuthorizationMiddleware>();
            builder.UseMiddleware<FunctionTypedResultsMiddleware>();
        },
        configureOptions: static _ => { })
    .ConfigureServices(services =>
    {
        services.AddDbContext<IdentityDbContext>();

        services.AddScoped<AuthenticationContext>();

        services.AddAdapters();
        services.AddAuth();
        services.AddSwagger();

        services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<HttpDataAccessor>();
        
        services.AddDataProtection(static options => options.ApplicationDiscriminator = "Blaganet.Identity");
    })
    .Build();

host.Run();