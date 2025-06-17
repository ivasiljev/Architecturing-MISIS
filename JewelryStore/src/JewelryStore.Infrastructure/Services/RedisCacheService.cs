using StackExchange.Redis;
using Newtonsoft.Json;
using JewelryStore.Core.Interfaces;

namespace JewelryStore.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly IServer _server;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
        _server = redis.GetServer(redis.GetEndPoints().First());
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            if (!value.HasValue)
                return null;

            return JsonConvert.DeserializeObject<T>(value!);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var json = JsonConvert.SerializeObject(value);
            await _database.StringSetAsync(key, json, expiration);
        }
        catch (Exception)
        {
            // Log error in production
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
        }
        catch (Exception)
        {
            // Log error in production
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var keys = _server.Keys(pattern: pattern);
            await _database.KeyDeleteAsync(keys.ToArray());
        }
        catch (Exception)
        {
            // Log error in production
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception)
        {
            return false;
        }
    }
}