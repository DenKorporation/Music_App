using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Dtos;

public class UserReadDto
{
    [Required]
    public int Id { get; set; }

    [Required] 
    public string Login { get; set; } = "";
    
}