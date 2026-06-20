using CryptoExchange.Api.Common;
using CryptoExchange.Api.Interfaces.External;
using CryptoExchange.Api.Models;
using CryptoExchange.Api.Models.External;

namespace CryptoExchange.Api.Services.External
{
    public class CoinMarketCapService : ICoinMarketCapService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CoinMarketCapService> _logger;

        public CoinMarketCapService(IHttpClientFactory httpClientFactory, ILogger<CoinMarketCapService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        public async Task<Result<CoinQuote>> GetLatestQuoteAsync(string cryptoCode, CancellationToken cancellationToken = default)
        {
            var symbol = cryptoCode.Trim().ToUpper();

            var client = _httpClientFactory.CreateClient("CoinMarketCap");

            try
            {
                var response = await client.GetAsync($"v3/cryptocurrency/quotes/latest?symbol={symbol}", cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("CoinMarketCap API returned error status: {StatusCode}", response.StatusCode);
                    return Result<CoinQuote>.Failure($"CoinMarketCap API returned error status: {response.StatusCode}");
                }
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<CoinMarketCapResponse>(cancellationToken: cancellationToken);

                    var usdPrice = data?.Data?.FirstOrDefault()?.Quote?.FirstOrDefault(q => q.Symbol == "USD")?.Price;
                    if (usdPrice == null)
                    {
                        _logger.LogWarning("CoinMarketCap response did not contain data for symbol: {Symbol}", symbol);
                        return Result<CoinQuote>.Failure($"CoinMarketCap response did not contain data for symbol: {symbol}");
                    }

                    return Result<CoinQuote>.Success(new CoinQuote() { Symbol = symbol, Price = usdPrice.Value, UpdatedAt = DateTime.UtcNow });
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Request for {Symbol} was cancelled by the client.", symbol);
                return Result<CoinQuote>.Failure("Request was cancelled.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "An error occurred while calling CoinMarketCap API for symbol: {Symbol}", symbol);
                return Result<CoinQuote>.Failure($"An error occurred while calling CoinMarketCap API for symbol: {symbol}");
            }
            
            return Result<CoinQuote>.Failure($"Unable to fetch live data for {symbol}.");
        }
    }
}