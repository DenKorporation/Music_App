using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models;

public class User
{
    [Key]
    [Required]
    public int Id { get; set; }

    [Required] 
    public string Login { get; set; } = "";
    
    
    // sha256(sha256(password)+salt)
    [StringLength(64)]
    [Required]
    public string Password { get; set; } = "";

    [StringLength(64)]
    [Required] 
    public string Salt { get; set; } = "";
    
    
}