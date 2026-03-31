namespace IdentityService.Helpers
{
    /// <summary>
    /// Represent or implements PasswordHasher.
    /// </summary>
    public class PasswordHasher
    {
        /// <summary>
        /// Performs the PasswordHash operation.
        /// </summary>
        public static string PasswordHash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        /// <summary>
        /// Performs the Verify operation.
        /// </summary>
        public static bool Verify(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
