using Blaganet.Identity;
using Blaganet.Identity.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(
        configure: static (_, builder) =>
        {
            builder.UseMiddleware<TypedResultsMiddleware>();
            builder.UseMiddleware<AuthorizationMiddleware>();
        },
        configureOptions: static _ => { })
    .ConfigureServices(services =>    {
        services.AddDbContext<IdentityDbContext>();

        services.AddScoped<AuthorizationContext>();

        services.AddAuth();
        services.AddSwagger();

        services.AddDataProtection(static options =>
        {
            options.ApplicationDiscriminator = IdentityDefaults.ServiceName;
        });
    })
    .Build();

host.Run();