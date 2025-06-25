using BloggingPlatform.API.DTOs;
using BloggingPlatform.API.Models;

namespace BloggingPlatform.API.Services
{
    public interface IUserService
    {
        Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDto);
        Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto);
        Task<AuthResponseDTO> RequestPasswordResetAsync(PasswordResetRequestDTO resetRequestDto);
        Task<AuthResponseDTO> ResetPasswordAsync(PasswordResetDTO resetDto);
        Task<AuthResponseDTO> ChangePasswordAsync(int userId, ChangePasswordDTO changePasswordDto);
        Task<UserDTO?> GetUserByIdAsync(int id);
        Task<User?> GetUserEntityByIdAsync(int id);
        Task<User?> GetUserEntityByEmailAsync(string email);
        Task<UserDTO?> UpdateProfileAsync(int userId, UpdateProfileDTO updateProfileDto);
        Task<AuthResponseDTO> SendOtpAsync(string email);
        bool VerifyOtp(string email, string otp);
    }
}