using System.Net;

namespace AuthenticationService.AuthenticationProvider.Models;

public class ErrorMessage
{
    public HttpStatusCode Error { get; set; }
    public string Message { get; set; } = "";
}