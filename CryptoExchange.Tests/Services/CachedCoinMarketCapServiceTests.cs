using CryptoExchange.Api.Common;
using CryptoExchange.Api.Configuration;
using CryptoExchange.Api.Interfaces.External;
using CryptoExchange.Api.Models;
using CryptoExchange.Api.Services.External.Cache;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CryptoExchange.Tests.Services
{
    public class CachedCoinMarketCapServiceTests : IDisposable
    {
        private readonly Mock<ICoinMarketCapService> _innerServiceMock;
        private readonly Mock<ILogger<CachedCoinMarketCapService>> _loggerMock;
        private readonly IMemoryCache _realCache;
        private readonly CacheSettings _settings;
        private readonly CachedCoinMarketCapService _sut;

        public CachedCoinMarketCapServiceTests()
        {
            _innerServiceMock = new Mock<ICoinMarketCapService>();
            _loggerMock = new Mock<ILogger<CachedCoinMarketCapService>>();
            _realCache = new MemoryCache(new MemoryCacheOptions());
            _settings = new CacheSettings
            {
                CoinMarketCap = new CoinMarketCapCacheSettings
                {
                    EnableCaching = true,
                    FreshnessTtlMinutes = 15,
                    AbsoluteExpirationHours = 24
                }
            };

            var optionsWrapper = Options.Create(_settings);

            _sut = new CachedCoinMarketCapService(
                _innerServiceMock.Object,
                _realCache,
                _loggerMock.Object,
                optionsWrapper);
        }

        // Cleanup the cache 
        public void Dispose()
        {
            _realCache.Dispose();
        }

        [Fact]
        public async Task GetLatestQuoteAsync_WhenDataIsFresh_ShouldNotCallApi()
        {
            var cryptoCode = "BTC";
            var cacheKey = $"CMC_Quote_{cryptoCode}";

            var freshData = new CoinQuote
            {
                Symbol = "BTC",
                Price = 50000m,
                UpdatedAt = DateTime.UtcNow.AddMinutes(-5) // Only 5 minutes old (Fresh!)
            };

            _realCache.Set(cacheKey, freshData);

            var result = await _sut.GetLatestQuoteAsync(cryptoCode);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Price.Should().Be(50000m);

            //Verify the real API was NEVER called!
            _innerServiceMock.Verify(x => x.GetLatestQuoteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetLatestQuoteAsync_WhenDataIsStaleAndApiSucceeds_ShouldCallApiAndUpdateCache()
        {
            // Arrange
            var cryptoCode = "BTC";
            var cacheKey = $"CMC_Quote_{cryptoCode}";

            var staleData = new CoinQuote
            {
                Symbol = "BTC",
                Price = 20000m,
                UpdatedAt = DateTime.UtcNow.AddMinutes(-30) // 30 minutes old (Stale!)
            };

            _realCache.Set(cacheKey, staleData);

            var newApiData = new CoinQuote { Symbol = "BTC", Price = 60000m, UpdatedAt = DateTime.UtcNow };

            // Setup the inner API to succeed and return the new data
            _innerServiceMock
                .Setup(x => x.GetLatestQuoteAsync(cryptoCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<CoinQuote>.Success(newApiData));

            // Act
            var result = await _sut.GetLatestQuoteAsync(cryptoCode);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Price.Should().Be(60000m);

            // Verify the real API WAS called 
            _innerServiceMock.Verify(x => x.GetLatestQuoteAsync(cryptoCode, It.IsAny<CancellationToken>()), Times.Once);

            // Verify the cache was actually updated with the new value
            _realCache.TryGetValue(cacheKey, out CoinQuote? cachedAfter).Should().BeTrue();
            cachedAfter!.Price.Should().Be(60000m);
        }

        [Fact]
        public async Task GetLatestQuoteAsync_WhenApiFailsButStaleDataExists_ShouldReturnStaleFallbackData()
        {
            // Arrange
            var cryptoCode = "BTC";
            var cacheKey = $"CMC_Quote_{cryptoCode}";

            var staleData = new CoinQuote
            {
                Symbol = "BTC",
                Price = 50000m,
                UpdatedAt = DateTime.UtcNow.AddHours(-2) // 2 Hours old (Very Stale!)
            };

            _realCache.Set(cacheKey, staleData);

            _innerServiceMock
                .Setup(x => x.GetLatestQuoteAsync(cryptoCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<CoinQuote>.Failure("API Offline"));

            // Act
            var result = await _sut.GetLatestQuoteAsync(cryptoCode);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Price.Should().Be(50000m);

            // Verify the API was attempted
            _innerServiceMock.Verify(x => x.GetLatestQuoteAsync(cryptoCode, It.IsAny<CancellationToken>()), Times.Once);
        }

    }
}