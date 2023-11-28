using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Dtos;

public class UserLoginDto
{
    [Required] 
    public string Login { get; set; } = "";

    [Required] 
    public string Password { get; set; } = "";
}