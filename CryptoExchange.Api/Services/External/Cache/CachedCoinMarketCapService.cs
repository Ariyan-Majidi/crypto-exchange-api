using CryptoExchange.Api.Common;
using CryptoExchange.Api.Configuration;
using CryptoExchange.Api.Interfaces.External;
using CryptoExchange.Api.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CryptoExchange.Api.Services.External.Cache
{
    public class CachedCoinMarketCapService : ICoinMarketCapService
    {
        private readonly ICoinMarketCapService _innerService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachedCoinMarketCapService> _logger;

        private readonly CoinMarketCapCacheSettings _cacheConfig;

        public CachedCoinMarketCapService(
            ICoinMarketCapService innerService,
            IMemoryCache cache,
            ILogger<CachedCoinMarketCapService> logger,
            IOptions<CacheSettings> cacheOptions)
        {
            _innerService = innerService;
            _cache = cache;
            _logger = logger;
            _cacheConfig = cacheOptions.Value.CoinMarketCap;
        }
        public async Task<Result<CoinQuote>> GetLatestQuoteAsync(string cryptoCode, CancellationToken cancellationToken = default)
        {
            var symbol = cryptoCode.Trim().ToUpper();
            var cacheKey = $"CMC_Quote_{symbol}";

            if (!_cacheConfig.EnableCaching)
            {
                return await _innerService.GetLatestQuoteAsync(symbol, cancellationToken);
            }

            var hasCachedData = _cache.TryGetValue(cacheKey, out CoinQuote? cachedQuote);

            if (hasCachedData && cachedQuote is not null &&
                (DateTime.UtcNow - cachedQuote.UpdatedAt).TotalMinutes < _cacheConfig.FreshnessTtlMinutes)
            {
                _logger.LogInformation("Returning HOT cached data for {Symbol}", symbol);
                return Result<CoinQuote>.Success(cachedQuote);
            }

            var liveResult = await _innerService.GetLatestQuoteAsync(symbol);

            if (liveResult.IsSuccess)
            {
                _cache.Set(cacheKey, liveResult.Data, TimeSpan.FromHours(_cacheConfig.AbsoluteExpirationHours));
                return liveResult;
            }


            if (hasCachedData && cachedQuote is not null)
            {
                _logger.LogWarning(
                    "Live API failed. Falling back to COLD cached data for {Symbol} from {Time}",
                    symbol,
                    cachedQuote.UpdatedAt);

                return Result<CoinQuote>.Success(cachedQuote);
            }

            return Result<CoinQuote>.Failure($"Live API failed and no cached data is available for {symbol}. Original Error: {liveResult.ErrorMessage}");
        }
    }
}