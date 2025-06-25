using BloggingPlatform.API.Data;
using BloggingPlatform.API.DTOs;
using BloggingPlatform.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;

namespace BloggingPlatform.API.Services
{
    // Static class to store OTPs across requests
    public static class OtpStore
    {
        // Use case-insensitive dictionary for email addresses
        private static Dictionary<string, (string otp, DateTime expiry)> _store;
        
        // Store for recently verified OTPs (for a short window to allow registration)
        private static Dictionary<string, (string otp, DateTime verifiedAt)> _verifiedOtps;
        
        // Static constructor to ensure initialization happens only once
        static OtpStore()
        {
            _store = new Dictionary<string, (string otp, DateTime expiry)>(StringComparer.OrdinalIgnoreCase);
            _verifiedOtps = new Dictionary<string, (string otp, DateTime verifiedAt)>(StringComparer.OrdinalIgnoreCase);
            Console.WriteLine("OtpStore static constructor called - OTP store initialized");
        }
        
        public static Dictionary<string, (string otp, DateTime expiry)> Store
        {
            get
            {
                if (_store == null)
                {
                    _store = new Dictionary<string, (string otp, DateTime expiry)>(StringComparer.OrdinalIgnoreCase);
                    Console.WriteLine("OtpStore.Store getter initialized a new dictionary");
                }
                return _store;
            }
        }
        
        // Method to add an OTP with detailed logging
        public static void AddOtp(string email, string otp, DateTime expiry)
        {
            if (_store == null)
            {
                _store = new Dictionary<string, (string otp, DateTime expiry)>(StringComparer.OrdinalIgnoreCase);
                Console.WriteLine("AddOtp method initialized a new dictionary");
            }
            
            _store[email] = (otp, expiry);
            Console.WriteLine($"AddOtp: Added OTP for {email}, store now contains {_store.Count} entries");
        }
        
        // Method to verify if an OTP exists and is valid
        public static bool VerifyOtp(string email, string otp, out (string otp, DateTime expiry) storedOtp)
        {
            if (_store == null)
            {
                _store = new Dictionary<string, (string otp, DateTime expiry)>(StringComparer.OrdinalIgnoreCase);
                Console.WriteLine("VerifyOtp method initialized a new dictionary");
                storedOtp = (string.Empty, DateTime.MinValue);
                return false;
            }
            
            Console.WriteLine($"VerifyOtp: Checking OTP for {email}, store contains {_store.Count} entries");
            return _store.TryGetValue(email, out storedOtp);
        }
        
        // Method to remove an OTP and store it as verified
        public static void RemoveOtp(string email)
        {
            if (_store != null && _store.ContainsKey(email))
            {
                var otp = _store[email].otp;
                _store.Remove(email);
                
                // Store in verified OTPs for a short window
                if (_verifiedOtps == null)
                {
                    _verifiedOtps = new Dictionary<string, (string otp, DateTime verifiedAt)>(StringComparer.OrdinalIgnoreCase);
                }
                
                _verifiedOtps[email] = (otp, DateTime.UtcNow);
                Console.WriteLine($"RemoveOtp: Removed OTP for {email} and stored as verified, store now contains {_store.Count} entries");
            }
        }
        
        // Method to check if an OTP was recently verified (within 2 minutes)
        public static bool WasRecentlyVerified(string email, string otp)
        {
            if (_verifiedOtps == null || !_verifiedOtps.ContainsKey(email))
            {
                Console.WriteLine($"WasRecentlyVerified: No verified OTP found for {email}");
                return false;
            }
            
            var verifiedOtp = _verifiedOtps[email];
            
            // Check if verification was within the last 2 minutes
            if (DateTime.UtcNow.Subtract(verifiedOtp.verifiedAt).TotalMinutes > 2)
            {
                Console.WriteLine($"WasRecentlyVerified: Verified OTP for {email} has expired");
                _verifiedOtps.Remove(email);
                return false;
            }
            
            // Check if OTP matches
            var isValid = verifiedOtp.otp == otp;
            Console.WriteLine($"WasRecentlyVerified: OTP comparison for {email} - Input: {otp}, Verified: {verifiedOtp.otp}, Match: {isValid}");
            
            if (isValid)
            {
                // Remove from verified OTPs after successful registration
                _verifiedOtps.Remove(email);
                Console.WriteLine($"WasRecentlyVerified: Removed verified OTP for {email} after successful check");
            }
            
            return isValid;
        }
    }

    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public UserService(ApplicationDbContext context, ITokenService tokenService, IEmailService emailService)
        {
            _context = context;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO registerDto)
        {
            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Email is already registered."
                };
            }

            // Create new user
            var user = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = _tokenService.GenerateJwtToken(user);

            return new AuthResponseDTO
            {
                Success = true,
                Message = "Registration successful.",
                Token = token,
                User = new UserDTO
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                }
            };
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO loginDto)
        {
            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            // Check if user exists and password is correct
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Invalid email or password."
                };
            }

            // Update last login timestamp
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = _tokenService.GenerateJwtToken(user);

            return new AuthResponseDTO
            {
                Success = true,
                Message = "Login successful.",
                Token = token,
                User = new UserDTO
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email
                }
            };
        }

        private string GenerateOtp()
        {
            // Generate a 6-digit OTP
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task<AuthResponseDTO> SendOtpAsync(string email)
        {
            // Normalize email address to lowercase
            email = email.Trim().ToLowerInvariant();
            Console.WriteLine($"SendOtpAsync: Normalized email to {email}");
            
            // For registration, we should send OTP to any email, not just existing users
            // Generate OTP
            var otp = GenerateOtp();
            Console.WriteLine($"Generated OTP for {email}: {otp}");
            
            // Store OTP with 10-minute expiry
            var expiryTime = DateTime.UtcNow.AddMinutes(10);
            OtpStore.AddOtp(email, otp, expiryTime);
            Console.WriteLine($"Stored OTP for {email} with expiry at {expiryTime}");
            
            // Debug: Print all entries in the OTP store
            foreach (var entry in OtpStore.Store)
            {
                Console.WriteLine($"OTP store entry: Email={entry.Key}, OTP={entry.Value.otp}, Expiry={entry.Value.expiry}");
            }
            
            try
            {
                Console.WriteLine($"Attempting to send OTP email to {email}");
                // Send OTP via email
                await _emailService.SendOtpEmailAsync(email, otp);
                
                Console.WriteLine($"Successfully sent OTP email to {email}");
                string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "email_logs.txt");
                File.AppendAllText(logFilePath, $"[{DateTime.Now}] Successfully sent OTP to {email}\n");
                
                return new AuthResponseDTO
                {
                    Success = true,
                    Message = "OTP has been sent to your email. Please use it within 10 minutes."
                };
            }
            catch (Exception ex)
            {
                // Log the exception with detailed information
                var errorMessage = $"Failed to send OTP to {email}: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner exception: {ex.InnerException.Message}";
                }
                
                Console.WriteLine(errorMessage);
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "email_logs.txt");
                File.AppendAllText(logFilePath, $"[{DateTime.Now}] {errorMessage}\n");
                
                // For debugging purposes, print the OTP to console
                Console.WriteLine($"DEBUG - OTP for {email} was: {otp}");
                
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Failed to send OTP. Please try again later. Error: " + ex.Message
                };
            }
        }

        public bool VerifyOtp(string email, string otp)
        {
            // Normalize email address to lowercase
            email = email.Trim().ToLowerInvariant();
            Console.WriteLine($"VerifyOtp: Normalized email to {email}");
            
            // First check if this OTP was recently verified (for registration flow)
            if (OtpStore.WasRecentlyVerified(email, otp))
            {
                Console.WriteLine($"VerifyOtp: OTP for {email} was recently verified and is still valid");
                return true;
            }
            
            // Check if OTP exists for the email
            if (!OtpStore.VerifyOtp(email, otp, out var storedOtp))
            {
                Console.WriteLine($"VerifyOtp: No OTP found for {email}");
                
                // Debug: Print all entries in the OTP store
                foreach (var entry in OtpStore.Store)
                {
                    Console.WriteLine($"OTP store entry: Email={entry.Key}, OTP={entry.Value.otp}, Expiry={entry.Value.expiry}");
                }
                
                return false;
            }

            // Check if OTP is expired
            if (storedOtp.expiry < DateTime.UtcNow)
            {
                Console.WriteLine($"VerifyOtp: OTP for {email} expired at {storedOtp.expiry}, current time is {DateTime.UtcNow}");
                OtpStore.RemoveOtp(email);
                return false;
            }

            // Check if OTP matches
            var isValid = storedOtp.otp == otp;
            Console.WriteLine($"VerifyOtp: OTP comparison for {email} - Input: {otp}, Stored: {storedOtp.otp}, Match: {isValid}");

            // Remove OTP after use (one-time use)
            if (isValid)
                OtpStore.RemoveOtp(email);

            return isValid;
        }

        public async Task<AuthResponseDTO> RequestPasswordResetAsync(PasswordResetRequestDTO resetRequestDto)
        {
            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetRequestDto.Email);

            if (user == null)
            {
                // For security reasons, don't reveal that the email doesn't exist
                Console.WriteLine($"RequestPasswordResetAsync: Email {resetRequestDto.Email} not found in database");
                return new AuthResponseDTO
                {
                    Success = true,
                    Message = "If your email is registered, you will receive a password reset link."
                };
            }

            // Generate password reset token
            var token = _tokenService.GeneratePasswordResetToken(user.Email);
            Console.WriteLine($"Generated password reset token for {user.Email}");

            try
            {
                Console.WriteLine($"Attempting to send password reset email to {user.Email}");
                // Send password reset email
                await _emailService.SendPasswordResetEmailAsync(user.Email, token);
                
                Console.WriteLine($"Successfully sent password reset email to {user.Email}");
                string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "email_logs.txt");
                File.AppendAllText(logFilePath, $"[{DateTime.Now}] Successfully sent password reset email to {user.Email}\n");
                
                return new AuthResponseDTO
                {
                    Success = true,
                    Message = "If your email is registered, you will receive a password reset link."
                };
            }
            catch (Exception ex)
            {
                // Log the exception with detailed information
                var errorMessage = $"Failed to send password reset email to {user.Email}: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner exception: {ex.InnerException.Message}";
                }
                
                Console.WriteLine(errorMessage);
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "email_logs.txt");
                File.AppendAllText(logFilePath, $"[{DateTime.Now}] {errorMessage}\n");
                
                // For debugging purposes, print part of the token
                if (token.Length > 10)
                {
                    Console.WriteLine($"DEBUG - Token for {user.Email} starts with: {token.Substring(0, 10)}...");
                }
                
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Failed to send password reset email. Please try again later. Error: " + ex.Message
                };
            }
        }

        public async Task<AuthResponseDTO> ResetPasswordAsync(PasswordResetDTO resetDto)
        {
            Console.WriteLine($"ResetPasswordAsync: Attempting to reset password for {resetDto.Email}");
            Console.WriteLine($"ResetPasswordAsync: Token: {resetDto.Token.Substring(0, Math.Min(10, resetDto.Token.Length))}...");
            Console.WriteLine($"ResetPasswordAsync: NewPassword length: {resetDto.NewPassword.Length}, ConfirmNewPassword length: {resetDto.ConfirmNewPassword.Length}");
            
            // Validate token
            if (!_tokenService.ValidatePasswordResetToken(resetDto.Token, resetDto.Email))
            {
                Console.WriteLine($"ResetPasswordAsync: Token validation failed for {resetDto.Email}");
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Invalid or expired token."
                };
            }
            
            Console.WriteLine($"ResetPasswordAsync: Token validation successful for {resetDto.Email}");

            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetDto.Email);

            if (user == null)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetDto.NewPassword);
            await _context.SaveChangesAsync();

            return new AuthResponseDTO
            {
                Success = true,
                Message = "Password has been reset successfully."
            };
        }

        public async Task<UserDTO?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return null;

            return new UserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }

        public async Task<User?> GetUserEntityByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserEntityByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<UserDTO?> UpdateProfileAsync(int userId, UpdateProfileDTO updateProfileDto)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return null;

            // Update user properties
            user.FirstName = updateProfileDto.FirstName;
            user.LastName = updateProfileDto.LastName;

            await _context.SaveChangesAsync();

            return new UserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }

        public async Task<AuthResponseDTO> ChangePasswordAsync(int userId, ChangePasswordDTO changePasswordDto)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Current password is incorrect."
                };
            }

            // Update password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            await _context.SaveChangesAsync();

            return new AuthResponseDTO
            {
                Success = true,
                Message = "Password has been changed successfully."
            };
        }
    }
}