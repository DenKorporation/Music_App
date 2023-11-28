using System.Security.Cryptography;
using System.Text;

namespace AuthenticationService.Security;

public class PasswordHasher
{
    public static string HashPassword(string password, string salt)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            string saltedPassword = password + salt;
            byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
        }
    }

    public static string GenerateSalt()
    {
        byte[] saltBytes = new byte[32];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        
        return BitConverter.ToString(saltBytes).Replace("-", "").ToLower();
    }
}