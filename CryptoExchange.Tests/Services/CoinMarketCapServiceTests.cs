using System.Net;
using System.Text.Json;
using CryptoExchange.Api.Models.External;
using CryptoExchange.Api.Services.External;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace CryptoExchange.Tests.Services
{
public class CoinMarketCapServiceTests
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<ILogger<CoinMarketCapService>> _loggerMock;
        private readonly CoinMarketCapService _sut;

        public CoinMarketCapServiceTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _loggerMock = new Mock<ILogger<CoinMarketCapService>>();

            var httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.fake-cmc.com/")
            };

            _httpClientFactoryMock
                .Setup(x => x.CreateClient("CoinMarketCap"))
                .Returns(httpClient);

            _sut = new CoinMarketCapService(_httpClientFactoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetLatestQuoteAsync_WhenApiReturnsSuccess_ShouldReturnSuccessResultWithData()
        {
            var cryptoCode = "BTC";
            
            var fakeApiResponse = new CoinMarketCapResponse
            {
                Data = new List<CmcCryptoData>
                {
                    {
                        new CmcCryptoData()
                        {
                            Quote = new List<CmcQuote>()
                            {
                                new CmcQuote()
                                {
                                    Price = 65000m,
                                    Symbol = "USD"
                                }
                            }
                        }
                    }
                }
            };

            var jsonPayload = JsonSerializer.Serialize(fakeApiResponse);

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _sut.GetLatestQuoteAsync(cryptoCode);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Symbol.Should().Be("BTC");
            result.Data.Price.Should().Be(65000m);
        }

        [Fact]
        public async Task GetLatestQuoteAsync_WhenApiReturnsHttpError_ShouldReturnFailureResult()
        {
            // Arrange
            var cryptoCode = "BTC";

            // Simulate an upstream server error (500 Internal Server Error)
            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            // Act
            var result = await _sut.GetLatestQuoteAsync(cryptoCode);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Contain("CoinMarketCap API returned error");
            result.Data.Should().BeNull();
        }
    }
}