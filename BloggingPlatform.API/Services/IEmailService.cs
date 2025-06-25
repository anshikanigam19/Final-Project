using System.Threading.Tasks;

namespace BloggingPlatform.API.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
        Task SendPasswordResetEmailAsync(string to, string resetToken);
        Task SendOtpEmailAsync(string to, string otp);
    }
}