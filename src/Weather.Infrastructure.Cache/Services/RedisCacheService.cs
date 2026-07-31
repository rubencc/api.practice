using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Weather.Infrastructure.Cache.Configuration;

namespace Weather.Infrastructure.Cache.Services;

//TODO: Add instrumentation for monitoring
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _cache;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger,
        IOptions<RedisSettings> settings)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _cache = _redis.GetDatabase();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            IncludeFields = false,
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
        };
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            bool exists = await ExistsAsync(key).ConfigureAwait(false);

            if (!exists)
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }
            
            var cachedValue = await _cache.StringGetAsync(key).ConfigureAwait(false);
            
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(cachedValue.ToString(), _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving from cache. Key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            await _cache.StringSetAsync(key, serializedValue, expiration , When.Always, CommandFlags.None).ConfigureAwait(false);

            _logger.LogDebug("Cached value for key: {Key}, Expiration: {Expiration}",
                key, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache. Key: {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var value = await _cache.StringGetAsync(key).ConfigureAwait(false);
            return !string.IsNullOrEmpty(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache existence. Key: {Key}", key);
            return false;
        }
    }

    public async Task<bool> RemoveAsync(string key)
    {
        try
        {
            RedisKey redisKey = new RedisKey(key);
            await _cache.StringDeleteAsync(redisKey,When.Exists, CommandFlags.None).ConfigureAwait(false);
            _logger.LogDebug("Removed cache entry for key: {Key}", key);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache. Key: {Key}", key);
            return false;
        }
    }
    

    public async Task<bool> RemoveByPatternAsync(string pattern)
    {
        try
        {
            RedisKey redisKey = new RedisKey($"*{pattern}*");
            await _cache.StringDeleteAsync(redisKey,When.Exists, CommandFlags.None).ConfigureAwait(false);
            _logger.LogDebug("Removed cache entry for key: {Key}", pattern);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache. Key: {Key}", pattern);
            return false;
        }
    }
}