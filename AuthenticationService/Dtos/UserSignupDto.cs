namespace AuthenticationService.Dtos;

using System.ComponentModel.DataAnnotations;

public class UserSignupDto
{
    [Required] 
    public string Login { get; set; } = "";
    
    [Required] 
    public string Password { get; set; } = "";
}