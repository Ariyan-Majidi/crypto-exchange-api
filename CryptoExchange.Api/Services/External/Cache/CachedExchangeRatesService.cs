using CryptoExchange.Api.Common;
using CryptoExchange.Api.Configuration;
using CryptoExchange.Api.Interfaces.External;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CryptoExchange.Api.Services.External.Cache
{
    public class CachedExchangeRatesService : IExchangeRatesService
    {
        private const string CacheKey = "ExchangeRates_Latest";

        private readonly IExchangeRatesService _innerService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachedExchangeRatesService> _logger;
        private readonly ExchangeRatesCacheSettings _cacheConfig;

        public CachedExchangeRatesService(
            IExchangeRatesService innerService,
            IMemoryCache cache,
            ILogger<CachedExchangeRatesService> logger,
            IOptions<CacheSettings> options)
        {
            _innerService = innerService;
            _cache = cache;
            _logger = logger;
            _cacheConfig = options.Value.ExchangeRates;
        }

        public async Task<Result<ExchangeRates>> GetLatestRatesAsync(CancellationToken cancellationToken = default)
        {
            if (!_cacheConfig.EnableCaching)
            {
                return await _innerService.GetLatestRatesAsync(cancellationToken);
            }

            var hasCachedData = _cache.TryGetValue(CacheKey, out ExchangeRates? cachedData);

            if (hasCachedData && cachedData is not null &&
                (DateTime.UtcNow - cachedData.UpdatedAt).TotalHours < _cacheConfig.FreshnessTtlHours)
            {
                _logger.LogInformation("Returning HOT cached data for Exchange Rates.");
                return Result<ExchangeRates>.Success(cachedData);
            }

            var liveResult = await _innerService.GetLatestRatesAsync();

            if (liveResult.IsSuccess)
            {
                _cache.Set(CacheKey, liveResult.Data, TimeSpan.FromHours(_cacheConfig.AbsoluteExpirationHours));
                return liveResult;
            }

            if (hasCachedData && cachedData is not null)
            {
                _logger.LogWarning(
                    "Live API failed. Falling back to COLD cached data for Exchange Rates from {Time}",
                    cachedData.UpdatedAt);

                return Result<ExchangeRates>.Success(cachedData);
            }

            return Result<ExchangeRates>.Failure($"Live API failed and no cached Exchange Rates are available. Error: {liveResult.ErrorMessage}");
        }
    }
}