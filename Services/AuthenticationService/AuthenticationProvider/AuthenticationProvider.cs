using System.IdentityModel.Tokens.Jwt;
using System.Net;
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

public class AuthenticationProvider : IAuthenticationProvider
{
    private readonly UserDbContext _dbContext;
    private readonly ILogger<AuthenticationProvider> _logger;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    private const string RegisterUser = "register_user";
    private const string AdminUser = "admin";

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
                { Error = HttpStatusCode.Unauthorized, Message = "Invalid username or password." };
            return false;
        }
        _logger.LogInformation($"User '{user.Id}' '{user.Login}' login successfully");
        jwt = GenerateJwt(user);
        return true;
    }

    private string GenerateJwt(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Login),
            new(ClaimTypes.Role, user.Role?.Name ?? "Undefined")
        };
        
        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _configuration["Issuer"],
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(10)),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["IssuerSigningKey"]!)),
                SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
    }

    public bool SignUp(UserSignupDto userSignupDto, out ErrorMessage? errorMessage)
    {
        errorMessage = null;
        var user = _dbContext.Users.FirstOrDefault(p => p.Login == userSignupDto.Login);
        if (user != null)
        {
            _logger.LogInformation($"User '{user.Login}' sign up failed");
            errorMessage = new ErrorMessage
                { Error = HttpStatusCode.Conflict, Message = "Login is already taken. Please choose another one." };
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
        if (!userClaims.IsInRole(AdminUser) && userClaims.Identity!.Name != userLogin)
        {
            _logger.LogInformation($"Failed to delete user {userLogin}, because no rights to perform this action");
            errorMessage = new ErrorMessage
                { Error = HttpStatusCode.Unauthorized, Message = "There are no rights to perform this action." };
            return false;
        }

        var user = _dbContext.Users.FirstOrDefault(p => p.Login == userLogin);
        if (user is null)
        {
            _logger.LogInformation($"Failed to delete user{userLogin}: Not Found");
            errorMessage = new ErrorMessage
                { Error = HttpStatusCode.NotFound, Message = "The user with the given login was not found." };
            return false;
        }

        _dbContext.Users.Remove(user);
        _dbContext.SaveChanges();
        
        _logger.LogInformation($"User '{user.Id.ToString()}' '{userLogin}' has been deleted successfully");
        return true;
    }

    public List<UserReadDto> GetUsers()
    {
        var users = _dbContext.Users.Include(p => p.Role).ToList()
                                .Select(s =>
                                {
                                    var userReadDto = _mapper.Map<UserReadDto>(s);
                                    userReadDto.Role =
                                        _dbContext.Roles.FirstOrDefault(p => p.Id == s.RoleId)?.Name ?? "Undefined";
                                    return userReadDto;
                                }).ToList();

        _logger.LogInformation("Users list sent successfully ");
        return users;
    }

    public List<RoleReadDto> GetRoles(ClaimsPrincipal userClaims)
    {
        var roles = _dbContext.Roles.ToList()
            .Select(s => _mapper.Map<RoleReadDto>(s)).ToList();

        if (!userClaims.IsInRole(AdminUser))
        {
            roles = roles.Where(s => s.Name != AdminUser).ToList();
        }
        _logger.LogInformation("Roles list sent successfully ");
        return roles;
    }

    public bool ChangeRole(ClaimsPrincipal userClaims, string userLogin, byte roleId, out string? jwt, out ErrorMessage? errorMessage)
    {
        errorMessage = null;
        jwt = null;
        byte adminId = _dbContext.Roles.First(p => p.Name == AdminUser).Id;
        if (userClaims.IsInRole(AdminUser))
        {
            var adminUsers = _dbContext.Users.Where(p => p.RoleId == adminId).ToList();
            // forbid if last admin try change his role
            if (adminUsers.Count == 1 && userLogin == adminUsers.First().Login && roleId != adminId)
            {
                _logger.LogWarning("Last Admin try change his role");
                errorMessage = new ErrorMessage
                    { Error = HttpStatusCode.Forbidden, Message = "Forbid to try delete last admin" };
                return false;
            }

            var user = _dbContext.Users.FirstOrDefault(p => p.Login == userLogin);
            if (user is null)
            {
                _logger.LogInformation($"Failed to change role for User {userLogin}: not found");
                errorMessage = new ErrorMessage
                    { Error = HttpStatusCode.NotFound, Message = "The user with the given login was not found." };
                return false;
            }

            user.RoleId = roleId;
            _dbContext.SaveChanges();

            _logger.LogInformation($"Role of User {user.Login} has been changed successfully");
            
            if (userClaims.Identity!.Name == userLogin)
            {
                jwt = GenerateJwt(user);
            }
            
            return true;
        }
        if (userClaims.Identity!.Name == userLogin && roleId != adminId)
        {
            var user = _dbContext.Users.FirstOrDefault(p => p.Login == userLogin);
            if (user is null)
            {
                _logger.LogInformation($"Failed to change role for User {userLogin}: not found");
                errorMessage = new ErrorMessage
                    { Error = HttpStatusCode.NotFound, Message = "The user with the given login was not found." };
                return false;
            }

            user.RoleId = roleId;
            _dbContext.SaveChanges();
            
            _logger.LogInformation($"Role of User {user.Login} has been changed successfully");
            
            jwt = GenerateJwt(user);
            return true;
        }
        
        _logger.LogWarning("The User doesn't have the rights to change role");
        
        errorMessage = new ErrorMessage
            { Error = HttpStatusCode.Forbidden, Message = "There are no rights to perform this action." };
        return false;
    }
}