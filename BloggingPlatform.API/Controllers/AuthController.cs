using BloggingPlatform.API.DTOs;
using BloggingPlatform.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloggingPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("send-otp")]
        public async Task<ActionResult<AuthResponseDTO>> SendOtp(SendOtpDTO sendOtpDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.SendOtpAsync(sendOtpDto.Email);
            return Ok(result);
        }

        [HttpPost("verify-otp")]
        public ActionResult<OtpResponseDTO> VerifyOtp(VerifyOtpDTO verifyOtpDto)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"VerifyOtp endpoint: ModelState invalid for {verifyOtpDto.Email}");
                return BadRequest(ModelState);
            }

            Console.WriteLine($"VerifyOtp endpoint: Attempting to verify OTP for {verifyOtpDto.Email}, OTP: {verifyOtpDto.Otp}");
            var isVerified = _userService.VerifyOtp(verifyOtpDto.Email, verifyOtpDto.Otp);
            Console.WriteLine($"VerifyOtp endpoint: Verification result for {verifyOtpDto.Email}: {isVerified}");
            
            return Ok(new OtpResponseDTO
            {
                Success = true,
                IsVerified = isVerified,
                Message = isVerified ? "OTP verified successfully." : "Invalid or expired OTP."
            });
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDTO>> Register(RegisterWithOtpDTO registerDto)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine($"Register: ModelState invalid for {registerDto.Email}");
                return BadRequest(ModelState);
            }

            Console.WriteLine($"Register: Attempting to verify OTP for {registerDto.Email}, OTP: {registerDto.Otp}");
            // Verify OTP first
            var isOtpVerified = _userService.VerifyOtp(registerDto.Email, registerDto.Otp);
            Console.WriteLine($"Register: OTP verification result for {registerDto.Email}: {isOtpVerified}");
            
            if (!isOtpVerified)
            {
                Console.WriteLine($"Register: OTP verification failed for {registerDto.Email}");
                return BadRequest(new AuthResponseDTO
                {
                    Success = false,
                    Message = "Invalid or expired OTP. Please request a new OTP."
                });
            }
            
            Console.WriteLine($"Register: OTP verified successfully for {registerDto.Email}");

            // Convert to RegisterDTO
            var standardRegisterDto = new RegisterDTO
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                Password = registerDto.Password,
                ConfirmPassword = registerDto.ConfirmPassword
            };

            var result = await _userService.RegisterAsync(standardRegisterDto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(LoginDTO loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.LoginAsync(loginDto);

            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<AuthResponseDTO>> ForgotPassword(PasswordResetRequestDTO resetRequestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.RequestPasswordResetAsync(resetRequestDto);

            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<AuthResponseDTO>> ResetPassword(PasswordResetDTO resetDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.ResetPasswordAsync(resetDto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}