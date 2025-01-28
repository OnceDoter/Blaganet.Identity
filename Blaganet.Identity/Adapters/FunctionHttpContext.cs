using System.Security.Claims;
using Blaganet.Identity.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Blaganet.Identity.Adapters;

public class FunctionHttpContext(
    IServiceProvider provider,
    CancellationTokenSource abortHandle) : HttpContext
{
    public override IFeatureCollection Features { get; }
    public override HttpRequest Request { get; }
    public override HttpResponse Response { get; }
    public override ConnectionInfo Connection { get; }
    public override WebSocketManager WebSockets { get; }

    public override ClaimsPrincipal User { get; set; } = provider.GetRequiredService<AuthenticationContext>().Principal ?? new ClaimsPrincipal();

    public override IDictionary<object, object?> Items { get; set; } = new Dictionary<object, object?>();
    
    public override IServiceProvider RequestServices
    {
        get => provider;
        set => throw new NotSupportedException();
    }
    
    public override CancellationToken RequestAborted
    {
        get => abortHandle.Token;
        set => throw new NotSupportedException();
    }

    public override string TraceIdentifier { get; set; } = Guid.NewGuid().ToString();

    public override ISession Session { get; set; } = new FunctionSession();
    
    public override void Abort()
    {
        abortHandle.Cancel();
    }
}