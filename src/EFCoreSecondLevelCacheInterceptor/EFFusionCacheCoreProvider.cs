using EasyCaching.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZiggyCreatures.Caching.Fusion;

namespace EFCoreSecondLevelCacheInterceptor
{
    /// <summary>
    ///     Using ICacheManager as a cache service.
    /// </summary>
    public class EFFusionCacheCoreProvider : IEFCacheServiceProvider
    {
        private readonly EFCoreSecondLevelCacheSettings _cacheSettings;
        private readonly IFusionCache _fusionCachingProvider;
        private readonly IEFDebugLogger _logger;

        /// <summary>
        ///     Using IMemoryCache as a cache service.
        /// </summary>
        public EFFusionCacheCoreProvider(
            IOptions<EFCoreSecondLevelCacheSettings> cacheSettings,
            IServiceProvider serviceProvider,
            IEFDebugLogger logger)
        {
            if (cacheSettings == null)
            {
                throw new ArgumentNullException(nameof(cacheSettings));
            }

            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            _cacheSettings = cacheSettings.Value;
            _logger = logger;

            _fusionCachingProvider = serviceProvider.GetRequiredService<IFusionCache>();
        }

        /// <summary>
        ///     Removes the cached entries added by this library.
        /// </summary>
        public void ClearAllCachedEntries()
        {
            _fusionCachingProvider.
        }

        /// <summary>
        ///     Gets a cached entry by key.
        /// </summary>
        /// <param name="cacheKey">key to find</param>
        /// <returns>cached value</returns>
        /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
        public EFCachedData? GetValue(EFCacheKey cacheKey, EFCachePolicy cachePolicy)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Adds a new item to the cache.
        /// </summary>
        /// <param name="cacheKey">key</param>
        /// <param name="value">value</param>
        /// <param name="cachePolicy">Defines the expiration mode of the cache item.</param>
        public void InsertValue(EFCacheKey cacheKey, EFCachedData value, EFCachePolicy cachePolicy)
        {
            if (cacheKey is null)
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }

            if (cachePolicy is null)
            {
                throw new ArgumentNullException(nameof(cachePolicy));
            }

            if (value == null)
            {
                value = new EFCachedData { IsNull = true };
            }

            var keyHash = cacheKey.KeyHash;

            foreach (var rootCacheKey in cacheKey.CacheDependencies)
            {
                if (string.IsNullOrWhiteSpace(rootCacheKey))
                {
                    continue;
                }

                var items = _fusionCachingProvider.TryGet<HashSet<string>>(rootCacheKey);
                if (!items.HasValue)
                {
                    _fusionCachingProvider.Set(rootCacheKey,
                                             new HashSet<string>(StringComparer.OrdinalIgnoreCase) { keyHash },
                                             cachePolicy.CacheTimeout);
                }
                else
                {
                    items.Value.Add(keyHash);
                    _fusionCachingProvider.Set(rootCacheKey, items.Value, cachePolicy.CacheTimeout);
                }
            }

            _fusionCachingProvider.Set(keyHash, value, cachePolicy.CacheTimeout);
        }

        /// <summary>
        ///     Invalidates all of the cache entries which are dependent on any of the specified root keys.
        /// </summary>
        /// <param name="cacheKey">Stores information of the computed key of the input LINQ query.</param>
        public void InvalidateCacheDependencies(EFCacheKey cacheKey)
        {
            throw new NotImplementedException();
        }
    }
}
