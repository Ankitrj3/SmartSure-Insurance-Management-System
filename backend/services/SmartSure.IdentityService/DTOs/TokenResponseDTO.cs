namespace IdentityService.DTOs
{
    public class TokenResponseDTO
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public string Role { get; set; }
    }
}
