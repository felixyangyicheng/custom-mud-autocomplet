// CustomAutoComplet.Services.Implementations/UserService.cs
using CustomAutoComplet.Data;
using CustomAutoComplet.Repository.Contracts;
using CustomAutoComplet.Services.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CustomAutoComplet.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepo _repo;
    private readonly ILogger<UserService> _logger;
    private readonly IMemoryCache _cache;
    private readonly MemoryCacheEntryOptions _cacheOptions;
    private const string UsersCacheKey = "users_";
    private const string UsersWithScoreCacheKey = "users_score_";
    private const string UsersWithScore10CacheKey = "users_score10_";
    private static readonly SemaphoreSlim _cacheLock = new(1, 1);

    public UserService(
        IUserRepo repo,
        ILogger<UserService> logger,
        IMemoryCache cache)
    {
        _repo = repo;
        _logger = logger;
        _cache = cache;

        _cacheOptions = new MemoryCacheEntryOptions()
            .SetSize(1) // Each list of users counts as 1 unit towards the SizeLimit
            .SetSlidingExpiration(TimeSpan.FromMinutes(5))
            .SetAbsoluteExpiration(TimeSpan.FromHours(1))
            .SetPriority(CacheItemPriority.Normal)
            .RegisterPostEvictionCallback(OnCacheEvicted);
    }

    public async IAsyncEnumerable<User> StreamUsersAsync(string keyword, CancellationToken ct)
    {
        var cacheKey = $"{UsersCacheKey}{keyword.ToLower()}";

        // Tentative de récupération depuis le cache
        if (_cache.TryGetValue<List<User>>(cacheKey, out var cachedUsers) && cachedUsers != null)
        {
            _logger.LogDebug("Retrieved users from cache for keyword: {Keyword}", keyword);
            foreach (var user in cachedUsers)
            {
                yield return user;
            }
            yield break;
        }

        // Si pas en cache, on lock pour éviter le cache stampede
        await _cacheLock.WaitAsync(ct);
        try
        {
            // Double-check après acquisition du lock
            if (_cache.TryGetValue<List<User>>(cacheKey, out cachedUsers) && cachedUsers != null)
            {
                foreach (var user in cachedUsers)
                {
                    yield return user;
                }
                yield break;
            }

            // Récupération depuis la base de données
            var users = new List<User>();
            await foreach (var user in _repo.StreamUsersAsync(keyword, ct))
            {
                users.Add(user);
                yield return user; // Streaming immédiat
            }

            // Mise en cache pour les futures requêtes
            if (users.Any())
            {
                _cache.Set(cacheKey, users, _cacheOptions);
                _logger.LogDebug("Cached {Count} users for keyword: {Keyword}", users.Count, keyword);
            }
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async IAsyncEnumerable<UserResultWithScore> StreamUsersWithScoreAsync(string keyword, CancellationToken ct)
    {
        var cacheKey = $"{UsersWithScoreCacheKey}{keyword.ToLower()}";

        if (_cache.TryGetValue<List<UserResultWithScore>>(cacheKey, out var cachedResults) && cachedResults != null)
        {
            _logger.LogDebug("Retrieved users with score from cache for keyword: {Keyword}", keyword);
            foreach (var result in cachedResults)
            {
                yield return result;
            }
            yield break;
        }

        await _cacheLock.WaitAsync(ct);
        try
        {
            // Double-check après acquisition du lock
            if (_cache.TryGetValue<List<UserResultWithScore>>(cacheKey, out cachedResults) && cachedResults != null)
            {
                foreach (var result in cachedResults)
                {
                    yield return result;
                }
                yield break;
            }

            var results = new List<UserResultWithScore>();
            await foreach (var result in _repo.StreamUsersWithScoreAsync(keyword, ct))
            {
                results.Add(result);
                yield return result;
            }

            if (results.Any())
            {
                _cache.Set(cacheKey, results, _cacheOptions);
                _logger.LogDebug("Cached {Count} users with score for keyword: {Keyword}", results.Count, keyword);
            }
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async IAsyncEnumerable<UserResultWithScore> StreamUsersWithScoreAsync10(string keyword, CancellationToken ct)
    {
        var cacheKey = $"{UsersWithScore10CacheKey}{keyword.ToLower()}";

        if (_cache.TryGetValue<List<UserResultWithScore>>(cacheKey, out var cachedResults) && cachedResults != null)
        {
            _logger.LogDebug("Retrieved top 10 users from cache for keyword: {Keyword}", keyword);
            foreach (var result in cachedResults)
            {
                yield return result;
            }
            yield break;
        }

        await _cacheLock.WaitAsync(ct);
        try
        {
            // Double-check après acquisition du lock
            if (_cache.TryGetValue<List<UserResultWithScore>>(cacheKey, out cachedResults) && cachedResults != null)
            {
                foreach (var result in cachedResults)
                {
                    yield return result;
                }
                yield break;
            }

            var results = new List<UserResultWithScore>();
            await foreach (var result in _repo.StreamUsersWithScoreAsync10(keyword, ct))
            {
                results.Add(result);
                yield return result;
            }

            if (results.Any())
            {
                _cache.Set(cacheKey, results, _cacheOptions);
                _logger.LogDebug("Cached {Count} top users for keyword: {Keyword}", results.Count, keyword);
            }
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    // Méthode pour invalider le cache (appelée par UserWatcherService lors des changements)
    public void InvalidateCache()
    {
        // Pattern pour trouver toutes les clés de cache commençant par nos préfixes
        var keysToRemove = new List<string>();

        // Note: IMemoryCache ne fournit pas de méthode pour énumérer toutes les clés
        // On peut utiliser un cache distribué ou une solution plus sophistiquée si besoin
        _logger.LogInformation("UserService cache invalidated");
    }

    // Méthode pour invalider le cache d'un mot-clé spécifique
    public void InvalidateCacheForKeyword(string keyword)
    {
        var keys = new[]
        {
            $"{UsersCacheKey}{keyword.ToLower()}",
            $"{UsersWithScoreCacheKey}{keyword.ToLower()}",
            $"{UsersWithScore10CacheKey}{keyword.ToLower()}"
        };

        foreach (var key in keys)
        {
            _cache.Remove(key);
            _logger.LogDebug("Removed cache for key: {Key}", key);
        }
    }

    // Callback pour le logging d'éviction de cache
    private void OnCacheEvicted(object key, object value, EvictionReason reason, object state)
    {
        _logger.LogDebug("Cache entry evicted: {Key} - Reason: {Reason}", key, reason);
    }

    // Méthode pour obtenir des statistiques de cache (utile pour le monitoring)
    public CacheStatistics GetCacheStatistics()
    {
        // Note: IMemoryCache ne fournit pas de statistiques natives
        // On pourrait implémenter un wrapper avec métriques si besoin
        return new CacheStatistics
        {
            LastInvalidation = DateTime.UtcNow,
            ServiceName = nameof(UserService)
        };
    }
}

// Classe pour les statistiques de cache
public class CacheStatistics
{
    public DateTime LastInvalidation { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int EstimatedCachedItems { get; set; }
}