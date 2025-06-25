using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BloggingPlatform.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly bool _enableSsl;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            
            // Get email settings from configuration
            var emailSettings = _configuration.GetSection("EmailSettings");
            _smtpServer = emailSettings["SmtpServer"] ?? "";
            _smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
            _smtpUsername = emailSettings["SmtpUsername"] ?? "";
            _smtpPassword = emailSettings["SmtpPassword"] ?? "";
            _senderEmail = emailSettings["SenderEmail"] ?? "";
            _senderName = emailSettings["SenderName"] ?? "Blogging Platform";
            _enableSsl = bool.Parse(emailSettings["EnableSsl"] ?? "true");
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            try
            {
                // Check if email settings are configured
                if (string.IsNullOrEmpty(_smtpServer) || string.IsNullOrEmpty(_smtpUsername) || 
                    string.IsNullOrEmpty(_smtpPassword) || string.IsNullOrEmpty(_senderEmail))
                {
                    throw new InvalidOperationException("Email settings are not properly configured.");
                }

                // Create logs directory in the application root
                string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "email_logs.txt");
                string logDirectory = Path.GetDirectoryName(logFilePath) ?? "";
                if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                Console.WriteLine($"Attempting to send email to {to} with subject: {subject}");
                Console.WriteLine($"Using SMTP settings: Server={_smtpServer}, Port={_smtpPort}, Username={_smtpUsername}, SSL={_enableSsl}");
                
                // Log attempt with full path
                File.AppendAllText(logFilePath, $"[{DateTime.Now}] Attempting to send email to {to} with subject: {subject}\n");
                File.AppendAllText(logFilePath, $"[{DateTime.Now}] Using SMTP settings: Server={_smtpServer}, Port={_smtpPort}, Username={_smtpUsername}, SSL={_enableSsl}\n");

                var message = new MailMessage
                {
                    From = new MailAddress(_senderEmail, _senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                
                message.To.Add(new MailAddress(to));

                // Configure SMTP client for Gmail - property order is critical
                using var client = new SmtpClient();
                
                // Log detailed connection attempt
                Console.WriteLine($"Setting up SMTP client with: Host={_smtpServer}, Port={_smtpPort}, Username={_smtpUsername}");
                // Removed duplicate declaration of logFilePath
                File.AppendAllText(logFilePath, $"[{DateTime.Now}] Setting up SMTP client with: Host={_smtpServer}, Port={_smtpPort}, Username={_smtpUsername}\n");
                
                // Set properties in the correct order (very important for Gmail)
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false; // Must be set before Credentials
                client.Host = _smtpServer;
                client.Port = _smtpPort;
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                client.EnableSsl = _enableSsl;
                client.Timeout = 120000; // 120 seconds timeout

                // Log before sending
                Console.WriteLine("Sending email now...");
                File.AppendAllText(logFilePath, $"[{DateTime.Now}] Sending email now...\n");

                // Try to send the email
                await client.SendMailAsync(message);

                // Log success
                Console.WriteLine("Email sent successfully!");
                File.AppendAllText(logFilePath, $"[{DateTime.Now}] Email sent successfully to {to}\n");
            }
            catch (Exception ex)
            {
                // Log the exception details
                var errorMessage = $"Failed to send email: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner exception: {ex.InnerException.Message}";
                }
                
                Console.WriteLine(errorMessage);
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Use the same log file path as defined above
                string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "email_logs.txt");
                File.AppendAllText(logFilePath, $"[{DateTime.Now}] {errorMessage}\n");
                File.AppendAllText(logFilePath, $"[{DateTime.Now}] Stack trace: {ex.StackTrace}\n");
                
                // Additional debugging information for Gmail-specific issues
                if (ex.Message.Contains("5.7.8") || ex.Message.Contains("authentication") || ex.Message.Contains("login"))
                {
                    var gmailTip = "This appears to be a Gmail authentication issue. Make sure you're using an App Password if 2FA is enabled on your Gmail account.";
                    Console.WriteLine(gmailTip);
                    File.AppendAllText(logFilePath, $"[{DateTime.Now}] {gmailTip}\n");
                }
                
                throw; // Re-throw to let the caller handle it
            }
        }

        public async Task SendPasswordResetEmailAsync(string to, string resetToken)
        {
            // Ensure the token is properly encoded for URLs
            var encodedEmail = WebUtility.UrlEncode(to);
            var encodedToken = WebUtility.UrlEncode(resetToken);
            
            var clientAppUrl = _configuration["ClientAppUrl"] ?? "http://localhost:4200";
            var resetUrl = $"{clientAppUrl}/auth/reset-password?email={encodedEmail}&token={encodedToken}";
            
            // Enhanced logging for debugging
            Console.WriteLine($"Generated password reset URL: {resetUrl}");
            Console.WriteLine($"Token length: {resetToken.Length}, Email: {to}");
            Console.WriteLine($"Token preview: {resetToken.Substring(0, Math.Min(10, resetToken.Length))}...");
            
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "email_logs.txt");
            File.AppendAllText(logFilePath, $"[{DateTime.Now}] Generated password reset URL for {to}: {resetUrl}\n");
            File.AppendAllText(logFilePath, $"[{DateTime.Now}] Token length: {resetToken.Length}, Token preview: {resetToken.Substring(0, Math.Min(10, resetToken.Length))}...\n");
            
            var subject = "Reset Your Password";
            var body = $@"<html>
                <body>
                    <h2>Reset Your Password</h2>
                    <p>You have requested to reset your password. Please click the link below to reset your password:</p>
                    <p><a href='{resetUrl}'>Reset Password</a></p>
                    <p>If you did not request a password reset, please ignore this email.</p>
                    <p>This link will expire in 1 hour.</p>
                    <p>Debug info:</p>
                    <ul>
                        <li>Token preview: {resetToken.Substring(0, Math.Min(10, resetToken.Length))}...</li>
                        <li>Token length: {resetToken.Length}</li>
                        <li>Email: {to}</li>
                    </ul>
                </body>
            </html>";

            await SendEmailAsync(to, subject, body, true);
        }

        public async Task SendOtpEmailAsync(string to, string otp)
        {
            var subject = "Your Verification Code";
            var body = $@"<html>
                <body>
                    <h2>Email Verification</h2>
                    <p>Your verification code is:</p>
                    <h1 style='font-size: 24px; font-weight: bold; color: #4285f4;'>{otp}</h1>
                    <p>This code will expire in 10 minutes.</p>
                    <p>If you did not request this code, please ignore this email.</p>
                </body>
            </html>";

            await SendEmailAsync(to, subject, body, true);
        }
    }
}