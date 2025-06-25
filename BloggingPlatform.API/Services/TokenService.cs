using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BloggingPlatform.API.Models;
using BloggingPlatform.API.Data;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;

namespace BloggingPlatform.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        // Use a static dictionary as a cache for tokens in memory
        // This helps with performance but we'll also store tokens in the database for persistence
        private static readonly Dictionary<string, (string token, DateTime expiry)> _passwordResetTokensCache = new();

        public TokenService(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured"));
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName)
                }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"] ?? "60")),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GeneratePasswordResetToken(string email)
        {
            // Generate a random token
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var token = Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");

            // Store the token with expiry (1 hour)
            var expiryTime = DateTime.UtcNow.AddHours(1);
            _passwordResetTokensCache[email] = (token, expiryTime);

            // Store token in database for persistence
            StoreTokenInDatabase(email, token, expiryTime);

            return token;
        }

        private async void StoreTokenInDatabase(string email, string token, DateTime expiry)
        {
            try
            {
                // Check if there's an existing token for this email
                var existingToken = await _context.PasswordResetTokens
                    .FirstOrDefaultAsync(t => t.Email == email);

                if (existingToken != null)
                {
                    // Update existing token
                    existingToken.Token = token;
                    existingToken.ExpiryDate = expiry;
                }
                else
                {
                    // Create new token entry
                    _context.PasswordResetTokens.Add(new PasswordResetToken
                    {
                        Email = email,
                        Token = token,
                        ExpiryDate = expiry
                    });
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"Token for {email} stored in database successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error storing token in database: {ex.Message}");
            }
        }

        public bool ValidatePasswordResetToken(string token, string email)
        {
            Console.WriteLine($"Validating token for {email}. Token count in memory cache: {_passwordResetTokensCache.Count}");
            
            // First check in-memory cache for performance
            if (_passwordResetTokensCache.TryGetValue(email, out var cachedToken))
            {
                Console.WriteLine($"Found token in memory cache for {email}");
                
                // Check if token is expired
                if (cachedToken.expiry < DateTime.UtcNow)
                {
                    Console.WriteLine($"Token for {email} is expired (from cache)");
                    _passwordResetTokensCache.Remove(email);
                    RemoveTokenFromDatabase(email).Wait();
                    return false;
                }

                // Check if token matches
                var isValid = cachedToken.token == token;
                Console.WriteLine($"Token comparison for {email} (from cache): Match: {isValid}");

                // Remove token after use (one-time use)
                if (isValid)
                {
                    Console.WriteLine($"Token validated successfully for {email}. Removing from stores.");
                    _passwordResetTokensCache.Remove(email);
                    RemoveTokenFromDatabase(email).Wait();
                }

                return isValid;
            }
            
            // If not in cache, check database
            Console.WriteLine($"Token not found in memory cache for {email}, checking database");
            var dbToken = GetTokenFromDatabase(email).Result;
            
            if (dbToken == null)
            {
                Console.WriteLine($"No token found in database for {email}");
                return false;
            }

            Console.WriteLine($"Found token in database for {email}. Expiry: {dbToken.ExpiryDate}, Current time: {DateTime.UtcNow}");
            
            // Check if token is expired
            if (dbToken.ExpiryDate < DateTime.UtcNow)
            {
                Console.WriteLine($"Token for {email} is expired (from database)");
                RemoveTokenFromDatabase(email).Wait();
                return false;
            }

            // Check if token matches
            var isValidFromDb = dbToken.Token == token;
            Console.WriteLine($"Token comparison for {email} (from database): Match: {isValidFromDb}");

            // Remove token after use (one-time use)
            if (isValidFromDb)
            {
                Console.WriteLine($"Token validated successfully for {email}. Removing from database.");
                RemoveTokenFromDatabase(email).Wait();
            }

            return isValidFromDb;
        }

        private async Task<PasswordResetToken?> GetTokenFromDatabase(string email)
        {
            try
            {
                return await _context.PasswordResetTokens
                    .FirstOrDefaultAsync(t => t.Email == email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving token from database: {ex.Message}");
                return null;
            }
        }

        private async Task RemoveTokenFromDatabase(string email)
        {
            try
            {
                var token = await _context.PasswordResetTokens
                    .FirstOrDefaultAsync(t => t.Email == email);

                if (token != null)
                {
                    _context.PasswordResetTokens.Remove(token);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Token for {email} removed from database");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing token from database: {ex.Message}");
            }
        }

        public int? ValidateJwtToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured"));

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = int.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);

                return userId;
            }
            catch
            {
                // Return null if validation fails
                return null;
            }
        }
    }
}