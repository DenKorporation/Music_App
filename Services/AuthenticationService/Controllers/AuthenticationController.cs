using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthenticationService.Data;
using AuthenticationService.Dtos;
using AuthenticationService.Models;
using AuthenticationService.Security;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationService.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthenticationController : ControllerBase
{
    private readonly UserDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthenticationController> _logger;
    private readonly IConfiguration _configuration;

    public AuthenticationController(IMapper mapper, UserDbContext dbContext, ILogger<AuthenticationController> logger,
        IConfiguration configuration)
    {
        _mapper = mapper;
        _dbContext = dbContext;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost(Name = "LogIn")]
    public ActionResult LogIn(UserLoginDto userLoginDto)
    {
        var user = _dbContext.Users.Include(user => user.Role).FirstOrDefault(p => p.Login == userLoginDto.Login);
        if (user == null || user.Password != PasswordHasher.HashPassword(userLoginDto.Password, user.Salt))
        {
            _logger.LogInformation($"User '{user?.Id.ToString() ?? "Undefined"}' '{userLoginDto.Login}' login failed");
            return Unauthorized(new
            {
                error = "Unauthorized",
                message = "Invalid username or password"
            });
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Role, user.Role?.Name ?? "Undefined")
        };
        // создаем JWT-токен
        var jwt = new JwtSecurityToken(
            issuer: _configuration["Issuer"],
            audience: _configuration["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(10)),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["IssuerSigningKey"]!)),
                SecurityAlgorithms.HmacSha256));

        _logger.LogInformation($"User '{user.Id}' '{user.Login}' login successfully");
        return Ok(new { access_token = new JwtSecurityTokenHandler().WriteToken(jwt) });
    }

    [Authorize]
    [HttpPost(Name = "LogOut")]
    public ActionResult LogOut()
    {
        return Ok();
    }

    [HttpPost(Name = "SignUp")]
    public ActionResult SignUp(UserSignupDto userSignupDto)
    {
        var user = _dbContext.Users.FirstOrDefault(p => p.Login == userSignupDto.Login);
        if (user != null)
        {
            _logger.LogInformation($"User '{user.Login}' sign up failed");
            return Conflict(new
            {
                error = "Conflict",
                message = "Login is already taken. Please choose another one."
            });
        }

        user = _mapper.Map<User>(userSignupDto);
        user.Salt = PasswordHasher.GenerateSalt();
        user.Password = PasswordHasher.HashPassword(user.Password, user.Salt);

        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();

        _logger.LogInformation($"User '{user.Id}' '{user.Login}' sign up successfully");
        //http 201 created
        return StatusCode(201);
    }
}