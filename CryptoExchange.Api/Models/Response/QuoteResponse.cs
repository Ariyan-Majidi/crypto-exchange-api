using System.Text.Json.Serialization;

namespace CryptoExchange.Api.Models.Response
{
    public class QuoteResponse
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("rates")]
        public Dictionary<string, decimal> Rates { get; set; } = new();
        
        [JsonPropertyName("coin_price_last_updated")]
        public DateTime CoinPriceLastUpdatedAt { get; set; } = DateTime.UtcNow;
        [JsonPropertyName("exchange_rates_last_updated")]
        public DateTime ExchangeRatesLastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}