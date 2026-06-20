using CryptoExchange.Api.Common;
using CryptoExchange.Api.Services;

namespace CryptoExchange.Api.Interfaces.External
{
    public interface IExchangeRatesService
    {
        Task<Result<ExchangeRates>> GetLatestRatesAsync(CancellationToken cancellationToken = default);
    }
}