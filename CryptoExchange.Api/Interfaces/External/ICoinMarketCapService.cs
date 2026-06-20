using CryptoExchange.Api.Common;
using CryptoExchange.Api.Models;

namespace CryptoExchange.Api.Interfaces.External
{
    public interface ICoinMarketCapService
    {
        Task<Result<CoinQuote>> GetLatestQuoteAsync(string cryptoCode, CancellationToken cancellationToken = default);
    }
}