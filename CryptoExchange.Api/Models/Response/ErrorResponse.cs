namespace CryptoExchange.Api.Models.Response
{
public class ErrorResponse
    {
        public string Message { get; set; }
        public string Error { get; set; }
        public ErrorResponse(string error, string message)
        {
            Error = error;
            Message = message;
        }
    }
}