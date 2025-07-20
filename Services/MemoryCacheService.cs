using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Saigor.Configuration;
using Saigor.Utils;

namespace Saigor.Services;

/// <summary>
/// Serviço de cache em memória com configuração avançada
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly MemoryCacheEntryOptions _defaultOptions;

    public MemoryCacheService(
        IMemoryCache cache,
        ILogger<MemoryCacheService> logger,
        IOptions<AppSettings> appSettings)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Configuração padrão do cache
        _defaultOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            SlidingExpiration = TimeSpan.FromMinutes(10),
            Priority = CacheItemPriority.Normal
        };
    }

    /// <summary>
    /// Obtém um item do cache
    /// </summary>
    public T? Get<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Chave do cache não pode ser vazia");
            return default;
        }

        try
        {
            if (_cache.TryGetValue(key, out T? value))
            {
                _logger.LogDebug("Item encontrado no cache: {Key}", key);
                return value;
            }

            _logger.LogDebug("Item não encontrado no cache: {Key}", key);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter item do cache: {Key}", key);
            return default;
        }
    }

    /// <summary>
    /// Define um item no cache
    /// </summary>
    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Chave do cache não pode ser vazia");
            return;
        }

        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _defaultOptions.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = _defaultOptions.SlidingExpiration,
                Priority = _defaultOptions.Priority
            };
            
            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration;
            }

            _cache.Set(key, value, options);
            _logger.LogDebug("Item definido no cache: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao definir item no cache: {Key}", key);
        }
    }

    /// <summary>
    /// Remove um item do cache
    /// </summary>
    public void Remove(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Chave do cache não pode ser vazia");
            return;
        }

        try
        {
            _cache.Remove(key);
            _logger.LogDebug("Item removido do cache: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover item do cache: {Key}", key);
        }
    }

    /// <summary>
    /// Verifica se um item existe no cache
    /// </summary>
    public bool Exists(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Chave do cache não pode ser vazia");
            return false;
        }

        try
        {
            return _cache.TryGetValue(key, out _);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência do item no cache: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Método genérico para executar operações síncronas com tratamento de erro padronizado.
    /// </summary>
    private T ExecuteWithErrorHandling<T>(Func<T> operation, ILogger logger, T defaultValue, string errorMessage)
    {
        return ErrorHandlingHelper.ExecuteWithErrorHandling(operation, logger, defaultValue, errorMessage);
    }

    /// <summary>
    /// Obtém ou define um item no cache
    /// </summary>
    public T GetOrSet<T>(string key, Func<T> factory, TimeSpan? expiration = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Chave do cache não pode ser vazia");
            return factory();
        }

        return ExecuteWithErrorHandling(
            () =>
            {
                if (_cache.TryGetValue(key, out T? cachedValue))
                {
                    _logger.LogDebug("Item encontrado no cache: {Key}", key);
                    return cachedValue!;
                }

                _logger.LogDebug("Item não encontrado no cache, executando factory: {Key}", key);
                var value = factory();
                Set(key, value, expiration);
                return value;
            },
            _logger,
            factory(),
            $"Erro ao obter ou definir item no cache: {key}"
        );
    }

    /// <summary>
    /// Obtém ou define um item no cache de forma assíncrona
    /// </summary>
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            _logger.LogWarning("Chave do cache não pode ser vazia");
            return await factory();
        }

        return await ErrorHandlingHelper.ExecuteWithErrorHandlingAsync(
            async () =>
            {
                if (_cache.TryGetValue(key, out T? cachedValue))
                {
                    _logger.LogDebug("Item encontrado no cache: {Key}", key);
                    return cachedValue!;
                }

                _logger.LogDebug("Item não encontrado no cache, executando factory assíncrona: {Key}", key);
                var value = await factory();
                Set(key, value, expiration);
                return value;
            },
            _logger,
            await factory(),
            $"Erro ao obter ou definir item no cache: {key}"
        );
    }

    /// <summary>
    /// Limpa todo o cache
    /// </summary>
    public void Clear()
    {
        ExecuteWithErrorHandling(
            () =>
            {
                if (_cache is MemoryCache memoryCache)
                {
                    memoryCache.Compact(1.0);
                    _logger.LogInformation("Cache limpo com sucesso");
                }
                return true;
            },
            _logger,
            false,
            "Erro ao limpar cache"
        );
    }

    /// <summary>
    /// Obtém estatísticas do cache
    /// </summary>
    public CacheStatistics GetStatistics()
    {
        return ExecuteWithErrorHandling(
            () =>
            {
                if (_cache is MemoryCache memoryCache)
                {
                    var stats = memoryCache.GetCurrentStatistics();
                    return new CacheStatistics
                    {
                        TotalHits = stats?.TotalHits ?? 0,
                        TotalMisses = stats?.TotalMisses ?? 0,
                        CurrentEntryCount = stats?.CurrentEntryCount ?? 0,
                        TotalRequests = (stats?.TotalHits ?? 0) + (stats?.TotalMisses ?? 0)
                    };
                }
                return new CacheStatistics();
            },
            _logger,
            new CacheStatistics(),
            "Erro ao obter estatísticas do cache"
        );
    }
}

/// <summary>
/// Estatísticas do cache
/// </summary>
public class CacheStatistics
{
    public long TotalHits { get; set; }
    public long TotalMisses { get; set; }
    public long CurrentEntryCount { get; set; }
    public long TotalRequests { get; set; }
    
    public double HitRate => TotalRequests > 0 ? (double)TotalHits / TotalRequests : 0;
} 