using System.Threading.RateLimiting;
using CryptoExchange.Api.Configuration;
using CryptoExchange.Api.Extensions;
using CryptoExchange.Api.Interfaces;
using CryptoExchange.Api.Interfaces.External;
using CryptoExchange.Api.Middleware;
using CryptoExchange.Api.Services;
using CryptoExchange.Api.Services.External;
using CryptoExchange.Api.Services.External.Cache;

var builder = WebApplication.CreateBuilder(args);



builder.Services.Configure<CoinMarketCapSettings>(
    builder.Configuration.GetSection("CoinMarketCapSettings"));

builder.Services.Configure<ExchangeRatesSettings>(
    builder.Configuration.GetSection("ExchangeRatesSettings"));

builder.Services.Configure<CacheSettings>(
    builder.Configuration.GetSection("CacheSettings"));



builder.Services.AddScoped<ICoinMarketCapService, CoinMarketCapService>();
builder.Services.Decorate<ICoinMarketCapService, CachedCoinMarketCapService>();
builder.Services.AddScoped<IExchangeRatesService, ExchangeRatesService>();
builder.Services.Decorate<IExchangeRatesService, CachedExchangeRatesService>();

builder.Services.AddScoped<ICryptoAggregatorService, CryptoAggregatorService>();


builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


builder.Services.AddExternalApiClients(builder.Configuration);

builder.Services.AddSecurityPolicies();

var app = builder.Build();


app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts(); 
}
app.UseHttpsRedirection();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseCors("StrictFrontendPolicy");
app.UseRateLimiter();

app.MapControllers();


app.Run();

