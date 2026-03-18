namespace IdentityService.Helpers
{
    public class PasswordHasher
    {
        public static string PasswordHash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public static bool Verify(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
