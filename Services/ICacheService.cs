namespace Saigor.Services;

/// <summary>
/// Interface para serviços de cache
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Obtém um item do cache
    /// </summary>
    T? Get<T>(string key);

    /// <summary>
    /// Define um item no cache
    /// </summary>
    void Set<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Remove um item do cache
    /// </summary>
    void Remove(string key);

    /// <summary>
    /// Verifica se um item existe no cache
    /// </summary>
    bool Exists(string key);

    /// <summary>
    /// Obtém ou define um item no cache
    /// </summary>
    T GetOrSet<T>(string key, Func<T> factory, TimeSpan? expiration = null);

    /// <summary>
    /// Obtém ou define um item no cache de forma assíncrona
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    /// <summary>
    /// Limpa todo o cache
    /// </summary>
    void Clear();

    /// <summary>
    /// Obtém estatísticas do cache
    /// </summary>
    CacheStatistics GetStatistics();
} 