using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace Blaganet.Identity.Adapters;

public class FunctionSession : ISession
{
    public Task LoadAsync(CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public Task CommitAsync(CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value) => throw new NotImplementedException();

    public void Set(string key, byte[] value) => throw new NotImplementedException();

    public void Remove(string key) => throw new NotImplementedException();

    public void Clear() => throw new NotImplementedException();

    public bool IsAvailable => false;
    public string Id => string.Empty;
    public IEnumerable<string> Keys => [];
}