using CryptoExchange.Api.Common;
using CryptoExchange.Api.Models.Response;

namespace CryptoExchange.Api.Interfaces
{
    public interface ICryptoAggregatorService
    {
        Task<Result<QuoteResponse>> GetAggregatedQuoteAsync(string cryptoCode, CancellationToken cancellationToken = default);
    }
}