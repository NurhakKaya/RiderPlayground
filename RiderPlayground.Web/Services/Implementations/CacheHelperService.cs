using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using RiderPlayground.Web.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace RiderPlayground.Web.Services.Implementations;

public class CacheHelperService : ICacheHelperService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CacheHelperService> _logger;
    private readonly IMemoryCache _memoryCache;

    public CacheHelperService(IConfiguration configuration, ILogger<CacheHelperService> logger,
        IMemoryCache memoryCache)
    {
        _configuration = configuration;
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public int SharedCacheDurationInMinutes() =>
        int.TryParse(_configuration["SharedCacheDurationInMinutes"], out int duration) ? duration : 10;

    public TimeSpan GetPartialCacheDuration()
    {
        return TimeSpan.FromMinutes(SharedCacheDurationInMinutes());
    }

    public object? GetValue(string key)
    {
        try
        {
            if (!string.IsNullOrEmpty(key) && _memoryCache.TryGetValue(key, out object? value))
            {
                return value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetValue error for key: {key}, {ex.Message}", ex);
        }

        return null;
    }

    public void SetValueWithAbsoluteExpiration(string key, object? value, int expirationTime,
        bool addExpirationAsSeconds = false)
    {
        if (!string.IsNullOrEmpty(key) && value != null)
        {
            try
            {
                var expirationTimeSpan = GetTimeSpan(expirationTime, addExpirationAsSeconds);

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(expirationTimeSpan)
                    .SetAbsoluteExpiration(expirationTimeSpan);

                _memoryCache.Set(key, value, cacheEntryOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"SetValueWithAbsoluteExpiration error for key: {key}, expirationTime: {expirationTime}, {ex.Message}",
                    ex);
            }
        }
    }

    public void ClearCache(string key)
    {
        try
        {
            if (!string.IsNullOrEmpty(key))
            {
                _memoryCache.Remove(key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ClearCache error for key: {key}, {ex.Message}", ex);
        }
    }

    #region Private Methods

    private TimeSpan GetTimeSpan(int expirationTime, bool addExpirationAsSeconds)
    {
        if (addExpirationAsSeconds)
        {
            return TimeSpan.FromSeconds(expirationTime);
        }
        else
        {
            return TimeSpan.FromMinutes(expirationTime);
        }
    }

    #endregion
}