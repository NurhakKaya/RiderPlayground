namespace RiderPlayground.Web.Services.Interfaces;

public interface ICacheHelperService
{
    int SharedCacheDurationInMinutes();
    
    TimeSpan GetPartialCacheDuration();
    
    object? GetValue(string key);
    
    void SetValueWithAbsoluteExpiration(string key, object? value, int expirationTime, bool addExpirationAsSeconds=false);
    
    void ClearCache(string key);
}