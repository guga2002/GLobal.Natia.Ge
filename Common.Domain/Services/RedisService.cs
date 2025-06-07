using System.Text.Json;
using Common.Domain.Interface;
using StackExchange.Redis;

namespace Common.Domain.Services;

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly IDatabase _database;

    public RedisService(IConnectionMultiplexer redisConnection)
    {
        _redisConnection = redisConnection;
        _database = _redisConnection.GetDatabase();
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var jsonData = JsonSerializer.Serialize(value);
        await _database.StringSetAsync(key, jsonData, expiry);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var jsonData = await _database.StringGetAsync(key);
        return jsonData.HasValue ? JsonSerializer.Deserialize<T>(jsonData) : default;
    }

    public async Task<bool> DeleteAsync(string key)
    {
        return await _database.KeyDeleteAsync(key);
    }

}
