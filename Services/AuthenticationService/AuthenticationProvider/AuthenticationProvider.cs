using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthenticationService.AuthenticationProvider.Models;
using AuthenticationService.Data;
using AuthenticationService.Dtos;
using AuthenticationService.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationService.AuthenticationProvider;

public class AuthenticationProvider: IAuthenticationProvider
{
    private readonly UserDbContext _dbContext;
    private readonly ILogger<AuthenticationProvider> _logger;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    
    private const string RegisterUser = "register_user";
    
    public AuthenticationProvider(UserDbContext dbContext, ILogger<AuthenticationProvider> logger,
        IConfiguration configuration, IMapper mapper)
    {
        _dbContext = dbContext;
        _logger = logger;
        _configuration = configuration;
        _mapper = mapper;
    }

    public bool LogIn(UserLoginDto userLoginDto, out string? jwt, out ErrorMessage? errorMessage)
    {
        jwt = null;
        errorMessage = null;
        
        var user = _dbContext.Users.Include(user => user.Role).FirstOrDefault(p => p.Login == userLoginDto.Login);
        if (user == null || user.Password != PasswordHasher.HashPassword(userLoginDto.Password, user.Salt))
        {
            _logger.LogInformation($"User '{user?.Id.ToString() ?? "Undefined"}' '{userLoginDto.Login}' login failed");
            errorMessage = new ErrorMessage
                { Error = "Unauthorized", Message = "Invalid username or password." };
            return false;
        }

        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, user.Login),
            new (ClaimTypes.Role, user.Role?.Name ?? "Undefined")
        };
        // creating JWT
        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _configuration["Issuer"],
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(10)),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["IssuerSigningKey"]!)),
                SecurityAlgorithms.HmacSha256));
        
        _logger.LogInformation($"User '{user.Id}' '{user.Login}' login successfully");
        jwt = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        return true;
    }

    public bool SignUp(UserSignupDto userSignupDto, out ErrorMessage? errorMessage)
    {
        errorMessage = null;
        var user = _dbContext.Users.FirstOrDefault(p => p.Login == userSignupDto.Login);
        if (user != null)
        {
            _logger.LogInformation($"User '{user.Login}' sign up failed");
            errorMessage = new ErrorMessage
                { Error = "Conflict", Message = "Login is already taken. Please choose another one." };
            return false;
        }

        user = _mapper.Map<User>(userSignupDto);
        user.Salt = PasswordHasher.GenerateSalt();
        user.Password = PasswordHasher.HashPassword(user.Password, user.Salt);
        user.RoleId = _dbContext.Roles.FirstOrDefault(p => p.Name == RegisterUser)?.Id;

        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        _logger.LogInformation($"User '{user.Id}' '{user.Login}' sign up successfully");
        return true;
    }

    public bool DeleteUser(ClaimsPrincipal userClaims, string userLogin, out ErrorMessage? errorMessage)
    {
        errorMessage = null;
        if (!userClaims.IsInRole("admin") && userClaims.Identity!.Name != userLogin)
        {
            errorMessage = new ErrorMessage
                { Error = "Forbid", Message = "There are no rights to perform this action." };
            return false;
        }

        var user = _dbContext.Users.FirstOrDefault(p => p.Login == userLogin);
        if (user is null)
        {
            errorMessage = new ErrorMessage
                { Error = "NotFound", Message = "The user with the given login was not found." };
            return false;
        }
        _dbContext.Users.Remove(user);
        _dbContext.SaveChanges();
        return true;
    }
}