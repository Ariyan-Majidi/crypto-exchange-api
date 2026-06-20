namespace CryptoExchange.Api.Services
{
    public class ExchangeRates
    {
        public Dictionary<string, decimal> Rates { get; set; } = new();
        public DateTime UpdatedAt { get; set; }
    }
}