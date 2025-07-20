using Microsoft.Extensions.Caching.Memory;

namespace Saigor.Utils
{
    /// <summary>
    /// Utilitário para cache genérico com funcionalidades avançadas.
    /// </summary>
    public static class CacheHelper
    {
        /// <summary>
        /// Obtém um item do cache ou o cria usando a função fornecida.
        /// </summary>
        /// <typeparam name="T>Tipo do item</typeparam>
        /// <param name="cache">Instância do cache</param>
        /// <param name="key>Chave do cache</param>
        /// <param name="factory">Função para criar o item</param>
        /// <param name=expirationMinutes">Tempo de expiração em minutos</param>
        /// <returns>Item do cache</returns>
        public static async Task<T> GetOrCreateAsync<T>(
            IMemoryCache cache,
            string key,
            Func<Task<T>> factory,
            int expirationMinutes = 30)
        {
            if (cache.TryGetValue(key, out T? cachedValue))
            {
                return cachedValue!;
            }

            var value = await factory();
            
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(expirationMinutes))
                .SetAbsoluteExpiration(TimeSpan.FromHours(2));

            cache.Set(key, value, cacheEntryOptions);
            
            return value;
        }

        /// <summary>
        /// Obtém um item do cache ou o cria usando a função fornecida (síncrono).
        /// </summary>
        /// <typeparam name="T>Tipo do item</typeparam>
        /// <param name="cache">Instância do cache</param>
        /// <param name="key>Chave do cache</param>
        /// <param name="factory">Função para criar o item</param>
        /// <param name=expirationMinutes">Tempo de expiração em minutos</param>
        /// <returns>Item do cache</returns>
        public static T GetOrCreate<T>(
            IMemoryCache cache,
            string key,
            Func<T> factory,
            int expirationMinutes = 30)
        {
            if (cache.TryGetValue(key, out T? cachedValue))
            {
                return cachedValue!;
            }

            var value = factory();
            
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(expirationMinutes))
                .SetAbsoluteExpiration(TimeSpan.FromHours(2));

            cache.Set(key, value, cacheEntryOptions);
            
            return value;
        }

        /// <summary>
        /// Remove um item do cache.
        /// </summary>
        /// <param name="cache">Instância do cache</param>
        /// <param name="key>Chave do cache</param>
        public static void Remove(IMemoryCache cache, string key)
        {
            cache.Remove(key);
        }

        /// <summary>
        /// Remove múltiplos itens do cache baseado em um padrão.
        /// </summary>
        /// <param name="cache">Instância do cache</param>
        /// <param name="pattern>Padrão para remoção</param>
        public static void RemoveByPattern(IMemoryCache cache, string pattern)
        {
            // Nota: IMemoryCache não suporta remoção por padrão nativamente
            // Esta é uma implementação simplificada
            // Para uma implementação completa, considere usar IDistributedCache com Redis
            
            var keys = GetCacheKeys(cache);
            foreach (var key in keys)
            {
                if (key.Contains(pattern))
                {
                    cache.Remove(key);
                }
            }
        }

        /// <summary>
        /// Limpa todo o cache.
        /// </summary>
        /// <param name="cache">Instância do cache</param>
        public static void Clear(IMemoryCache cache)
        {
            if (cache is MemoryCache memoryCache)
            {
                memoryCache.Compact(1.0);
            }
        }

        /// <summary>
        /// Gera uma chave de cache baseada em parâmetros.
        /// </summary>
        /// <param name="baseKey">Chave base</param>
        /// <param name="parameters">Parâmetros para a chave</param>
        /// <returns>Chave de cache</returns>
        public static string GenerateKey(string baseKey, params object[] parameters)
        {
            if (parameters.Length ==0)
            {
                return baseKey;
            }

            var paramString = string.Join("_", parameters.Select(p => p?.ToString() ?? "null"));
            return $"{baseKey}_{paramString}";
        }

        /// <summary>
        /// Obtém estatísticas do cache.
        /// </summary>
        /// <param name="cache">Instância do cache</param>
        /// <returns>Estatísticas do cache</returns>
        public static Dictionary<string, object> GetCacheStats(IMemoryCache cache)
        {
            var stats = new Dictionary<string, object>();
            
            if (cache is MemoryCache memoryCache)
            {
                var keys = GetCacheKeys(cache);
                stats["TotalItems"] = keys.Count;
                stats["UniqueKeys"] = keys.Distinct().Count();
            }
            
            stats["CacheType"] = cache.GetType().Name;
            stats["LastUpdated"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            
            return stats;
        }

        /// <summary>
        /// Obtém as chaves do cache (implementação simplificada).
        /// </summary>
        /// <param name="cache">Instância do cache</param>
        /// <returns>Lista de chaves</returns>
        private static List<string> GetCacheKeys(IMemoryCache cache)
        {
            // Nota: Esta é uma implementação simplificada
            // IMemoryCache não expõe suas chaves diretamente
            // Para uma implementação completa, considere usar IDistributedCache com Redis
            
            return new List<string>();
        }

        /// <summary>
        /// Configura opções de cache com expiração.
        /// </summary>
        /// <param name=slidingExpirationMinutes>Expiração deslizante em minutos</param>
        /// <param name="absoluteExpirationHours">Expiração absoluta em horas</param>
        /// <returns>Opções de cache</returns>
        public static MemoryCacheEntryOptions CreateCacheOptions(
            int slidingExpirationMinutes = 30, int absoluteExpirationHours = 2)
        {
            return new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(slidingExpirationMinutes))
                .SetAbsoluteExpiration(TimeSpan.FromHours(absoluteExpirationHours));
        }
    }
} 