using System.Text.Json;
using Engrslan.Application.Contracts.Services;
using Engrslan.Domain.Shared.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;

namespace Engrslan.Application.Services;

public class MemoryCacheService : ICacheService, ISingletonService
{
    private readonly IMemoryCache _cache;
    private readonly HashSet<string> _keys = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cache.Get<T?>(key));
    }

    public async Task<T> GetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null)
            return cached;

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            cached = await GetAsync<T>(key, cancellationToken);
            if (cached != null)
                return cached;

            var value = await factory();
            await SetAsync(key, value, expiration, cancellationToken);
            return value;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions();
        
        if (expiration.HasValue)
            options.SetSlidingExpiration(expiration.Value);
        else
            options.SetSlidingExpiration(TimeSpan.FromMinutes(5));

        _cache.Set(key, value, options);
        
        _keys.Add(key);
        
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        _keys.Remove(key);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var keysToRemove = _keys.Where(k => k.StartsWith(prefix)).ToList();
        
        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _keys.Remove(key);
        }
        
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cache.TryGetValue(key, out _));
    }
}