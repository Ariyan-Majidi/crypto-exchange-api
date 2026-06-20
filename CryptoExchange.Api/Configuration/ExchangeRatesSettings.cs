namespace CryptoExchange.Api.Configuration
{
    public class ExchangeRatesSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string Symbols { get; set; } = string.Empty;
    }
}