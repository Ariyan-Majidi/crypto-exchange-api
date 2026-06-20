
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace CryptoExchange.Api.Models.Request
{
    
    public class GetQuoteRequest
    {
        [FromRoute(Name = "cryptoCode")]
        [Required(ErrorMessage = "Crypto code is required.")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Crypto code must be between 2 and 10 characters.")]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Crypto code can only contain letters.")]
        public string CryptoCode { get; set; } = string.Empty;
    }
}