using AuthenticationService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationService.Data;

public class UserDbContext: DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> opt) : base(opt)
    {
    }
    
    public DbSet<User> Users { get; set; }
}