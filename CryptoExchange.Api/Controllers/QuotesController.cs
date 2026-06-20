using CryptoExchange.Api.Interfaces;
using CryptoExchange.Api.Models.Request;
using CryptoExchange.Api.Models.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CryptoExchange.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("IpRateLimit")]
    public class QuotesController : ControllerBase
    {
        private readonly ICryptoAggregatorService _aggregatorService;

        public QuotesController(ICryptoAggregatorService aggregatorService)
        {
            _aggregatorService = aggregatorService;
        }

        [HttpGet("{cryptoCode}")]
        public async Task<IActionResult> GetQuote([FromRoute] GetQuoteRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _aggregatorService.GetAggregatedQuoteAsync(request.CryptoCode, cancellationToken);

            if (!result.IsSuccess)
            {
                var error = new ErrorResponse(
                    error: "Not Found",
                    message: result.ErrorMessage
                );
                return NotFound(error);
            }

            return Ok(result);
        }
    }
}
