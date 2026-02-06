using Fabrica.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Fabrica.Services;

public interface ILoginCacheService
{
    bool TryGetLoggedUser(string? sessionId, out LoggedUser? user);
    void SetLoggedUser(string sessionId, LoggedUser user);
    void Remove(string sessionId);
}

public class LoginCacheService : ILoginCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _options;
    private const string CacheKeyPrefix = "login_user_";

    public LoginCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
        _options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };
    }

    public bool TryGetLoggedUser(string? sessionId, out LoggedUser? user)
    {
        user = null;

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return false;
        }

        if (_memoryCache.TryGetValue(BuildKey(sessionId), out LoggedUser? cachedUser))
        {
            user = cachedUser;
            return true;
        }

        return false;
    }

    public void SetLoggedUser(string sessionId, LoggedUser user)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return;
        }

        _memoryCache.Set(BuildKey(sessionId), user, _options);
    }

    public void Remove(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return;
        }

        _memoryCache.Remove(BuildKey(sessionId));
    }

    private static string BuildKey(string sessionId) => $"{CacheKeyPrefix}{sessionId}";
}
