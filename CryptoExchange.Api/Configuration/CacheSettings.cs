namespace CryptoExchange.Api.Configuration
{
public class CacheSettings
    {
        public ExchangeRatesCacheSettings ExchangeRates { get; set; } = new();
        public CoinMarketCapCacheSettings CoinMarketCap { get; set; } = new();
    }

    public class ExchangeRatesCacheSettings
    {
        public int FreshnessTtlHours { get; set; } = 8;
        public int AbsoluteExpirationHours { get; set; } = 48;
        public bool EnableCaching { get; set; } = true;
    }

    public class CoinMarketCapCacheSettings
    {
        public int FreshnessTtlMinutes { get; set; } = 15;
        public int AbsoluteExpirationHours { get; set; } = 24;
        public bool EnableCaching { get; set; } = true;
    }
}