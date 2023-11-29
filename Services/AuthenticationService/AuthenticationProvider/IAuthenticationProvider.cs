using System.Security.Claims;
using AuthenticationService.AuthenticationProvider.Models;
using AuthenticationService.Dtos;

namespace AuthenticationService.AuthenticationProvider;

public interface IAuthenticationProvider
{
    public bool LogIn(UserLoginDto userLoginDto, out string? jwt, out ErrorMessage? errorMessage);
    public bool SignUp(UserSignupDto userSignupDto, out ErrorMessage? errorMessage);
    public bool DeleteUser(ClaimsPrincipal userClaims, string userLogin, out ErrorMessage? errorMessage);
}