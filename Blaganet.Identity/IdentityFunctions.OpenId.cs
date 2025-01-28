using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Blaganet.Identity;

internal partial class IdentityFunctions
{
    [Function(nameof(OpenIdConfiguration))]
    [OpenApiOperation(nameof(OpenIdConfiguration), "OpenId")]
    public Task<OpenIdConnectConfiguration> OpenIdConfiguration(
        [HttpTrigger(AuthorizationLevel.Anonymous,"get", Route = ".well-known/openid-configuration")] HttpRequestData requestData,
        CancellationToken ct)
    {
        var baseUrl = requestData.Url.GetLeftPart(UriPartial.Authority);
        return Task.FromResult(new OpenIdConnectConfiguration
        {
            Issuer = baseUrl,
            AuthorizationEndpoint = $"{baseUrl}/connect/authorize",
            TokenEndpoint = $"{baseUrl}/connect/token",
        });
    }
}