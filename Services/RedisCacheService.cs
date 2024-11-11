using System.Text.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

public class CacheService
{
    private readonly IDatabase _database;
    private readonly IOptions<CacheConfiguration> _options;

    public CacheService(IConnectionMultiplexer redis, IOptions<CacheConfiguration> options)
    {
        _database = redis.GetDatabase();
        _options = options;
    }

    public async Task<T> GetOrCreateAsync<T>(Action<CacheOptions<T>> options)
    {
        var cacheOptions = new CacheOptions<T>();
        options(cacheOptions);

        var cacheKey = string.Concat(_options.Value.Organization, ":", cacheOptions.Key);

        var data = GetCache(cacheKey);
        if (string.IsNullOrEmpty(data))
        {
            var newData = await cacheOptions.Reader();
            Cache(cacheKey, JsonSerializer.Serialize(newData), cacheOptions.Duration);
            return newData;
        }
        return JsonSerializer.Deserialize<T>(data) ?? default!;
    }

    private void Cache(string key, string value, TimeSpan? expiration = null)
    {
        _database.StringSet(key, value, expiration);
    }

    private string GetCache(string key)
    {
        var value = _database.StringGet(key);
        return value.HasValue ? value.ToString() : string.Empty;
    }

    public void FlushCache(string orgName)
    {
        var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: $"{_options.Value.Organization}{orgName}:*").ToArray();
        _database.KeyDelete(keys);
    }

}
