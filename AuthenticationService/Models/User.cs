using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationService.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    
    public string Login { get; set; } = "";
    
    // sha256(sha256(password)+salt)
    [StringLength(64)]
    public string Password { get; set; } = "";

    [StringLength(64)]
    public string Salt { get; set; } = "";
    
    public byte? RoleId { get; set; }
    
    public Role? Role { get; set; }
}