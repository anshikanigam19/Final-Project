using BloggingPlatform.API.Models;

namespace BloggingPlatform.API.Services
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user);
        string GeneratePasswordResetToken(string email);
        bool ValidatePasswordResetToken(string token, string email);
        int? ValidateJwtToken(string token);
    }
}