using CryptoExchange.Api.Common;
using CryptoExchange.Api.Configuration;
using CryptoExchange.Api.Interfaces.External;
using CryptoExchange.Api.Models.External;
using Microsoft.Extensions.Options;

namespace CryptoExchange.Api.Services.External
{
    public class ExchangeRatesService : IExchangeRatesService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ExchangeRatesService> _logger;
        private readonly IOptions<ExchangeRatesSettings> _settings;
        const string ApiErrorMessage = "An error occurred while calling the ExchangeRates API";

        public ExchangeRatesService(IHttpClientFactory httpClientFactory, ILogger<ExchangeRatesService> logger, IOptions<ExchangeRatesSettings> settings)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _settings = settings;
        }

        public async Task<Result<ExchangeRates>> GetLatestRatesAsync(CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient("ExchangeRates");
            var result = new ExchangeRates();
            var symbols = _settings.Value.Symbols;
            try
            {
                var response = await client.GetAsync($"latest?access_key={_settings.Value.ApiKey}&symbols={symbols}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<ExchangeRatesResponse>(cancellationToken: cancellationToken);


                    if (data?.Rates != null && data.Rates.TryGetValue("USD", out var eurToUsdRate) && eurToUsdRate > 0)
                    {
                        var normalizedRates = new Dictionary<string, decimal>();
                        data.Rates["EUR"] = 1.0m;
                        foreach (var rate in data.Rates)
                        {
                            normalizedRates[rate.Key] = rate.Value / eurToUsdRate;
                        }
                        result.Rates = normalizedRates;
                    }
                    result.UpdatedAt = DateTime.UtcNow;
                    return Result<ExchangeRates>.Success(result);
                }

                _logger.LogWarning("ExchangeRates API returned error status: {StatusCode}", response.StatusCode);

            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ExchangeRates request was cancelled by the client.");
                return Result<ExchangeRates>.Failure("Request was cancelled.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, ApiErrorMessage);
                return Result<ExchangeRates>.Failure(ApiErrorMessage);
            }

            return Result<ExchangeRates>.Failure(ApiErrorMessage);
        }
    }
}