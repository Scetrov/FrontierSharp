using Microsoft.Extensions.Caching.Hybrid;

namespace FrontierSharp.Tests;

public class FakeHybridCache : HybridCache {
    // Not sure if this should be static, nor do I know what to do about Tags
    // Could make public since it's a fake cache anyway, and that could be useful for testing purposes
    private readonly Dictionary<string, object?> _cache = new();

    public override async ValueTask<T> GetOrCreateAsync<TState, T>(string key, TState state, Func<TState, CancellationToken, ValueTask<T>> factory,
        HybridCacheEntryOptions? options = null,
        IEnumerable<string>? tags = null, CancellationToken cancellationToken = default) {
        var cached = _cache.TryGetValue(key, out var value);
        if (cached) return (T?)value!;
        _cache.Add(key, await factory(state, cancellationToken));
        return (T?)_cache[key]!;
    }

    public override ValueTask SetAsync<T>(string key, T value, HybridCacheEntryOptions? options = null, IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default) {
        _cache[key] = value;
        return ValueTask.CompletedTask;
    }

    public override ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default) {
        _cache.Remove(key);
        return ValueTask.CompletedTask;
    }

    public override ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default) {
        throw new NotImplementedException();
    }
}