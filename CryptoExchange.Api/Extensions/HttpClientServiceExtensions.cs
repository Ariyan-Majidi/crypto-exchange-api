using CryptoExchange.Api.Configuration;
using Polly;
using Polly.Extensions.Http;

namespace CryptoExchange.Api.Extensions
{
public static class HttpClientServiceExtensions
    {
        public static IServiceCollection AddExternalApiClients(this IServiceCollection services, IConfiguration config)
        {
            services.AddHttpClient("CoinMarketCap", client =>
            {
                var settings = config.GetSection("CoinMarketCapSettings").Get<CoinMarketCapSettings>();
                client.BaseAddress = new Uri(settings?.BaseUrl ?? throw new InvalidOperationException("CMC Base URL missing"));
                client.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", settings.ApiKey);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddHttpClient("ExchangeRates", client =>
            {
                var settings = config.GetSection("ExchangeRatesSettings").Get<ExchangeRatesSettings>();
                client.BaseAddress = new Uri(settings?.BaseUrl ?? throw new InvalidOperationException("ExchangeRates Base URL missing"));
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() 
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
        }
    }
}