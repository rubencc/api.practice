using System.Collections.Concurrent;

namespace Weather.Infrastructure.Cache.Services;

public class MemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, object> _cache = new();

    public MemoryCacheService()
    {
        _cache = new ConcurrentDictionary<string, object>();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (await ExistsAsync(key).ConfigureAwait(false))
        {
            return (T)_cache[key];
        }

        return default;
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        _cache.TryAdd(key, value);

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key)
    {
        return Task.FromResult(_cache.TryGetValue(key, out var value));
    }

    public Task<bool> RemoveAsync(string key)
    {
        var deleted = _cache.TryRemove(key, out _);
        return Task.FromResult(deleted);
    }

    public Task<bool> RemoveByPatternAsync(string pattern)
    {
        List<string> keys = _cache.Keys.ToList();
        List<string> findings = new List<string>();

        foreach (var keyValuePair in keys)
        {
            if (keyValuePair.Contains(pattern))
                findings.Add(keyValuePair);
        }

        foreach (var finding in findings)
        {
            Task.FromResult(_cache.TryRemove(finding, out _));
        }

        return Task.FromResult(findings.Count != 0);
    }
}