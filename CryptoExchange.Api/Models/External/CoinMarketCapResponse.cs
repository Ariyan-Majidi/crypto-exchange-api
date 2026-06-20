using System.Text.Json.Serialization;

namespace CryptoExchange.Api.Models.External
{
    public class CoinMarketCapResponse
    {
        [JsonPropertyName("status")]
        public CmcStatus? Status { get; set; }

        [JsonPropertyName("data")]
        public List<CmcCryptoData>? Data { get; set; }
    }

    public class CmcCryptoData
    {
        [JsonPropertyName("quote")]
        public List<CmcQuote>? Quote { get; set; }
    }

    public class CmcQuote
    {
        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }
    }

    public class CmcStatus
    {
        [JsonPropertyName("error_code")]
        public int ErrorCode { get; set; }

        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }
    }
}
