using System.Text.Json.Serialization;

namespace CryptoExchange.Api.Models.External
{
    public class ExchangeRatesResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("base")]
        public string? Base { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<string, decimal>? Rates { get; set; }

        [JsonPropertyName("error")]
        public ExchangeRateError? Error { get; set; }
    }

    public class ExchangeRateError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("info")]
        public string? Info { get; set; }
    }
}