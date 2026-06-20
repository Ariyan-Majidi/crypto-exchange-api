using CryptoExchange.Api.Common;
using CryptoExchange.Api.Interfaces;
using CryptoExchange.Api.Interfaces.External;
using CryptoExchange.Api.Models.Response;

namespace CryptoExchange.Api.Services
{
    public class CryptoAggregatorService : ICryptoAggregatorService
    {
        private readonly ICoinMarketCapService _cmcService;
        private readonly IExchangeRatesService _exchangeService;

        public CryptoAggregatorService(ICoinMarketCapService cmcService, IExchangeRatesService exchangeService)
        {
            _cmcService = cmcService;
            _exchangeService = exchangeService;
        }

        public async Task<Result<QuoteResponse>> GetAggregatedQuoteAsync(string cryptoCode, CancellationToken cancellationToken = default)
        {
            var symbol = cryptoCode.Trim().ToUpper();

            var cmcTask = _cmcService.GetLatestQuoteAsync(symbol, cancellationToken);
            var exchangeTask = _exchangeService.GetLatestRatesAsync(cancellationToken);

            await Task.WhenAll(cmcTask, exchangeTask);

            var cmcResult = await cmcTask;
            var exchangeResult = await exchangeTask;

            if (!cmcResult.IsSuccess)
                return Result<QuoteResponse>.Failure(cmcResult.ErrorMessage);

            if (!exchangeResult.IsSuccess)
                return Result<QuoteResponse>.Failure(exchangeResult.ErrorMessage);

            var usdPrice = cmcResult.Data.Price;
            var rates = exchangeResult.Data.Rates;

            var conversions = new Dictionary<string, decimal>();

            foreach (var currency in rates)
            {
                if (rates.TryGetValue(currency.Key, out var rate))
                {
                    var convertedPrice = usdPrice * rate;
                    conversions.Add(currency.Key, convertedPrice);
                }
            }

            var responseData = new QuoteResponse
            {
                Symbol = symbol,
                Rates = conversions,
                CoinPriceLastUpdatedAt = cmcResult.Data.UpdatedAt,
                ExchangeRatesLastUpdatedAt = exchangeResult.Data.UpdatedAt
            };

            return Result<QuoteResponse>.Success(responseData);
        }

    }
}