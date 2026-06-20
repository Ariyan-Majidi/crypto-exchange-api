namespace CryptoExchange.Api.Models
{
    public class CoinQuote
    {
        public required string Symbol { get; set; }
        public required decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime UpdatedAt { get; set; }
    }
}