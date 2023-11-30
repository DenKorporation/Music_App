using System.Net;
using AuthenticationService.AuthenticationProvider;
using AuthenticationService.AuthenticationProvider.Models;
using AuthenticationService.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthenticationController : ControllerBase
{
    private record Jwt(string AccessToken);

    private readonly IAuthenticationProvider _authenticationProvider;

    public AuthenticationController(IAuthenticationProvider authenticationProvider)
    {
        _authenticationProvider = authenticationProvider;
    }

    [HttpPost(Name = "LogIn")]
    [ProducesResponseType(typeof(ErrorMessage), (int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(Jwt), (int)HttpStatusCode.OK)]
    public ActionResult LogIn(UserLoginDto userLoginDto)
    {
        if (_authenticationProvider.LogIn(userLoginDto, out var jwt, out var errorMessage))
        {
            return Ok(new Jwt(jwt!));
        }

        return Unauthorized(errorMessage);
    }

    [Authorize]
    [HttpPost(Name = "LogOut")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public ActionResult LogOut()
    {
        return Ok();
    }

    [HttpPost(Name = "SignUp")]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorMessage), (int)HttpStatusCode.Conflict)]
    public ActionResult SignUp(UserSignupDto userSignupDto)
    {
        if (_authenticationProvider.SignUp(userSignupDto, out var errorMessage))
        {
            //http 201 created
            return StatusCode(201);
        }

        return Conflict(errorMessage);
    }
    
    [Authorize]
    [HttpPost("{userLogin}/{roleId:max(255)}", Name = "ChangeRole")]
    [ProducesResponseType(typeof(Jwt), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorMessage), (int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(ErrorMessage), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ErrorMessage), (int)HttpStatusCode.Forbidden)]
    public ActionResult Role(string userLogin, byte roleId)
    {
        var userClaims = ControllerContext.HttpContext.User;
        if (_authenticationProvider.ChangeRole(userClaims, userLogin, roleId, out var jwt, out var errorMessage))
        {
            if (jwt is null)
            {
                return Ok();       
            }

            return Ok(new Jwt(jwt));
        }

        return StatusCode((int)errorMessage!.Error, errorMessage);
    }

    [Authorize(Roles = "admin")]
    [HttpGet(Name = "GetUsers")]
    [ProducesResponseType(typeof(List<UserReadDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    public ActionResult Users()
    {
        var users = _authenticationProvider.GetUsers();

        return Ok(users);
    }

    [Authorize]
    [HttpGet(Name = "GetRoles")]
    [ProducesResponseType(typeof(List<RoleReadDto>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    public ActionResult Roles()
    {
        var roles = _authenticationProvider.GetRoles(ControllerContext.HttpContext.User);

        return Ok(roles);
    }
    
    [Authorize]
    [HttpDelete("{userLogin}", Name = "DeleteUser")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorMessage), (int)HttpStatusCode.Forbidden)]
    [ProducesResponseType(typeof(ErrorMessage), (int)HttpStatusCode.NotFound)]
    public ActionResult UserAccount(string userLogin)
    {
        var userClaims = ControllerContext.HttpContext.User;
        if (_authenticationProvider.DeleteUser(userClaims, userLogin, out var errorMessage))
        {
            return Ok();
        }

        return StatusCode((int)errorMessage!.Error, errorMessage);
    }
}