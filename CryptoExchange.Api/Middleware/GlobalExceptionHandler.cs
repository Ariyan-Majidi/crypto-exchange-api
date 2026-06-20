using CryptoExchange.Api.Models.Response;
using Microsoft.AspNetCore.Diagnostics;

namespace CryptoExchange.Api.Middleware
{
public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext, 
            Exception exception, 
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "A critical unhandled exception occurred: {Message}", exception.Message);

            var errorResponse = new ErrorResponse(
                error: "Internal Server Error",
                message: "An unexpected error occurred while processing your request. Please try again later."
            );

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

            return true;
        }
    }
}