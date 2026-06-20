using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace CryptoExchange.Api.Extensions
{
    public static class SecurityServiceExtensions
    {
        public static IServiceCollection AddSecurityPolicies(this IServiceCollection services)
        {
            // 1. Configure CORS
            services.AddCors(options =>
            {
                options.AddPolicy("StrictFrontendPolicy", policy =>
                {
                    policy.WithOrigins("https://my-crypto-dashboard.com", "http://localhost:5000") // Only allow these URLs
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // 2. Configure Rate Limiting
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddPolicy("IpRateLimit", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 30,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));
            });

            return services;
        }
    }
}