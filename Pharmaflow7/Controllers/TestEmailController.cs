using Microsoft.AspNetCore.Mvc;
using Pharmaflow7.Services;

namespace Pharmaflow7.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestEmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;
        private readonly ILogger<TestEmailController> _logger;

        public TestEmailController(IEmailService emailService, IOtpService otpService, ILogger<TestEmailController> logger)
        {
            _emailService = emailService;
            _otpService = otpService;
            _logger = logger;
        }

        /// <summary>
        /// Test basic email sending functionality
        /// Usage: GET /api/testemail/send-test?email=your-email@gmail.com
        /// </summary>
        [HttpGet("send-test")]
        public async Task<IActionResult> SendTestEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email parameter is required");
            }

            try
            {
                await _emailService.SendEmailAsync(email, "Test Email - PharmaFlow", 
                    "<h1>Test Email</h1><p>If you receive this email, your SMTP configuration is working correctly!</p>");
                
                _logger.LogInformation("Test email sent successfully to {Email}", email);
                return Ok(new { success = true, message = "Test email sent successfully! Check your inbox." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test email to {Email}", email);
                return StatusCode(500, new { 
                    success = false, 
                    message = "Failed to send test email", 
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Test OTP generation and sending
        /// Usage: GET /api/testemail/send-otp?email=your-email@gmail.com
        /// </summary>
        [HttpGet("send-otp")]
        public async Task<IActionResult> SendTestOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email parameter is required");
            }

            try
            {
                var otpCode = await _otpService.GenerateOtpAsync(email, "TestPurpose");
                _logger.LogInformation("Test OTP generated and sent to {Email}. Code: {OtpCode}", email, otpCode);
                
                return Ok(new { 
                    success = true, 
                    message = "Test OTP sent successfully! Check your inbox.",
                    otpCode = otpCode // Only for testing - remove in production
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test OTP to {Email}", email);
                return StatusCode(500, new { 
                    success = false, 
                    message = "Failed to send test OTP", 
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        /// <summary>
        /// Test SMTP configuration without sending email
        /// Usage: GET /api/testemail/test-config
        /// </summary>
        [HttpGet("test-config")]
        public IActionResult TestSmtpConfig([FromServices] IConfiguration configuration)
        {
            var emailSettings = configuration.GetSection("EmailSettings");
            
            var config = new
            {
                SmtpServer = emailSettings["SmtpServer"],
                SmtpPort = emailSettings["SmtpPort"],
                SenderEmail = emailSettings["SenderEmail"],
                SenderName = emailSettings["SenderName"],
                SmtpUsername = emailSettings["SmtpUsername"],
                HasPassword = !string.IsNullOrEmpty(emailSettings["SmtpPassword"]),
                PasswordLength = emailSettings["SmtpPassword"]?.Length ?? 0
            };

            var issues = new List<string>();
            
            if (string.IsNullOrEmpty(config.SmtpServer))
                issues.Add("SmtpServer is not configured");
            
            if (string.IsNullOrEmpty(config.SenderEmail) || config.SenderEmail.Contains("your-email"))
                issues.Add("SenderEmail contains placeholder values");
                
            if (string.IsNullOrEmpty(config.SmtpUsername) || config.SmtpUsername.Contains("your-email"))
                issues.Add("SmtpUsername contains placeholder values");
                
            if (!config.HasPassword || config.PasswordLength < 10)
                issues.Add("SmtpPassword appears to be placeholder or too short");

            return Ok(new { 
                configuration = config,
                issues = issues,
                isConfigurationValid = issues.Count == 0
            });
        }
    }
}