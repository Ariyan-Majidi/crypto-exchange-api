using CryptoExchange.Api.Common;
using CryptoExchange.Api.Interfaces.External;
using CryptoExchange.Api.Models;
using CryptoExchange.Api.Services;
using FluentAssertions;
using Moq;

namespace CryptoExchange.Tests.Services
{
    public class CryptoAggregatorServiceTests
    {
        private readonly Mock<ICoinMarketCapService> _cmcServiceMock;
        private readonly Mock<IExchangeRatesService> _exchangeServiceMock;
        private readonly CryptoAggregatorService _sut; 
        public CryptoAggregatorServiceTests()
        {
            _cmcServiceMock = new Mock<ICoinMarketCapService>();
            _exchangeServiceMock = new Mock<IExchangeRatesService>();
            _sut = new CryptoAggregatorService(_cmcServiceMock.Object, _exchangeServiceMock.Object);
        }

        [Fact]
        public async Task GetAggregatedQuoteAsync_WhenBothApisSucceed_ShouldCalculateCorrectConversions()
        {
            var cryptoCode = "BTC";
            var fakeCmcResponse = new CoinQuote 
            { 
                Symbol = "BTC", 
                Price = 50000m,
                UpdatedAt = DateTime.UtcNow 
            };
            
            var fakeExchangeResponse = new ExchangeRates
            {
                Rates = new Dictionary<string, decimal>
                {
                    { "EUR", 0.90m }, 
                    { "GBP", 0.80m }  
                },
                UpdatedAt = DateTime.UtcNow
            };

            _cmcServiceMock
                .Setup(x => x.GetLatestQuoteAsync(cryptoCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<CoinQuote>.Success(fakeCmcResponse));

            _exchangeServiceMock
                .Setup(x => x.GetLatestRatesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<ExchangeRates>.Success(fakeExchangeResponse));

            
            var result = await _sut.GetAggregatedQuoteAsync(cryptoCode);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Symbol.Should().Be("BTC");
            
            // 50,000 * 0.90 = 45,000
            result.Data.Rates["EUR"].Should().Be(45000m); 
            // 50,000 * 0.80 = 40,000
            result.Data.Rates["GBP"].Should().Be(40000m); 
        }

        [Fact]
        public async Task GetAggregatedQuoteAsync_WhenCmcFails_ShouldReturnFailureResult()
        {
            var cryptoCode = "BTC";
            
            // CMC API failing
            _cmcServiceMock
                .Setup(x => x.GetLatestQuoteAsync(cryptoCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<CoinQuote>.Failure("CMC API is down"));

            _exchangeServiceMock
                .Setup(x => x.GetLatestRatesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<ExchangeRates>.Success(new ExchangeRates()));

            var result = await _sut.GetAggregatedQuoteAsync(cryptoCode);

            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("CMC API is down");
            
            result.Data.Should().BeNull(); 
        }
    }
}