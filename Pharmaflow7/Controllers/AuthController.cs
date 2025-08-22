using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Pharmaflow7.Models;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Pharmaflow7.Services;
using System.Text.Encodings.Web;


namespace Pharmaflow7.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AuthController> _logger;
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, ILogger<AuthController> logger, IEmailService emailService, IOtpService otpService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _emailService = emailService;
            _otpService = otpService;
        }

        [HttpGet]
        [Route("api/auth/status")]
        public IActionResult GetAuthStatus()
        {
            return Ok(new { isAuthenticated = User.Identity.IsAuthenticated });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new UserRegistrationModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserRegistrationModel model)
        {
            _logger.LogInformation("📋 البيانات المُرسلة: Email={Email}, RoleType={RoleType}", model.Email, model.RoleType);

            ModelState.Clear();

            if (string.IsNullOrEmpty(model.Email))
                ModelState.AddModelError("Email", "Email is required.");
            else if (!model.Email.EndsWith(".com", StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError("Email", "Email must end with .com");
            if (string.IsNullOrEmpty(model.Password))
                ModelState.AddModelError("Password", "Password is required.");
            if (model.Password.Length < 8 || !model.Password.Any(char.IsUpper) || !model.Password.Any(char.IsDigit))
                ModelState.AddModelError("Password", "Password must be at least 8 characters, with an uppercase letter and a number.");
            if (string.IsNullOrEmpty(model.RoleType))
                ModelState.AddModelError("RoleType", "User type is required.");

            switch (model.RoleType)
            {
                case "driver":
                    if (string.IsNullOrEmpty(model.FullName))
                        ModelState.AddModelError("FullName", "Full Name is required for drivers.");
                    break;
                case "company":
                    if (string.IsNullOrEmpty(model.CompanyName))
                        ModelState.AddModelError("CompanyName", "Company Name is required for companies.");
                    if (string.IsNullOrEmpty(model.LicenseNumber))
                        ModelState.AddModelError("LicenseNumber", "License Number is required for companies.");
                    if (string.IsNullOrEmpty(model.CompanyContactNumber))
                        ModelState.AddModelError("CompanyContactNumber", "Contact Number is required for companies.");
                    break;
                case "distributor":
                    if (string.IsNullOrEmpty(model.DistributorName))
                        ModelState.AddModelError("DistributorName", "Distributor Name is required for distributors.");
                    if (string.IsNullOrEmpty(model.WarehouseAddress))
                        ModelState.AddModelError("WarehouseAddress", "Warehouse Address is required for distributors.");
                    if (string.IsNullOrEmpty(model.DistributorContactNumber))
                        ModelState.AddModelError("DistributorContactNumber", "Contact Number is required for distributors.");
                    break;
                default:
                    if (!string.IsNullOrEmpty(model.RoleType))
                        ModelState.AddModelError("RoleType", "Invalid user type.");
                    break;
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("❌ ModelState غير صالح");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                RoleType = model.RoleType,
                FullName = model.FullName,
                Address = model.Address,
                CompanyName = model.CompanyName,
                LicenseNumber = model.LicenseNumber,
                ContactNumber = model.RoleType == "company" ? model.CompanyContactNumber : model.RoleType == "distributor" ? model.DistributorContactNumber : null,
                DistributorName = model.DistributorName,
                WarehouseAddress = model.WarehouseAddress
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("✅ تم إنشاء المستخدم بنجاح: {Email}", user.Email);
                if (!await _roleManager.RoleExistsAsync(model.RoleType))
                {
                    await _roleManager.CreateAsync(new IdentityRole(model.RoleType));
                }
                await _userManager.AddToRoleAsync(user, model.RoleType);

                // Generate and send OTP for email verification
                try
                {
                    await _otpService.GenerateOtpAsync(user.Email, "EmailVerification");
                    _logger.LogInformation("📧 تم إرسال رمز التحقق إلى: {Email}", user.Email);
                    
                    TempData["SuccessMessage"] = "Registration successful! Please check your email for the verification code.";
                    return RedirectToAction("VerifyOtp", new { email = user.Email });
                }
                catch (Exception ex)
                {
                    _logger.LogError("❌ Full error when sending OTP to {Email}: {Exception}", user.Email, ex.ToString());
                    TempData["WarningMessage"] = $"Registration successful, but OTP failed: {ex.Message}";
                    return RedirectToAction("Login");
                }

            }

            _logger.LogError("❌ فشل إنشاء المستخدم: {Email}", user.Email);
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("تسجيل دخول ناجح لـ {Email}", model.Email);
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    return RedirectToDashboard(user.RoleType);
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("المستخدم {Email} تم قفله بسبب محاولات فاشلة", model.Email);
                    return RedirectToAction("Lockout");
                }
                if (result.IsNotAllowed)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
                    {
                        _logger.LogWarning("محاولة تسجيل دخول بدون تأكيد البريد الإلكتروني: {Email}", model.Email);
                        
                        // Generate new OTP and redirect to verification
                        try
                        {
                            await _otpService.GenerateOtpAsync(user.Email, "EmailVerification");
                            TempData["InfoMessage"] = "Please verify your email address first. We've sent a new verification code to your email.";
                            return RedirectToAction("VerifyOtp", new { email = user.Email });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to generate OTP for unverified user: {Email}", user.Email);
                            ModelState.AddModelError(string.Empty, "Your email is not verified. Please contact support.");
                            return View(model);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Auth", new { returnUrl }, protocol: "https");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            _logger.LogInformation("Starting External Login for {Provider} with redirect URL: {RedirectUrl}", provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            try
            {
                if (remoteError != null)
                {
                    _logger.LogWarning("خطأ من External Provider: {Error}", remoteError);
                    return RedirectToAction("Login");
                }

                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    _logger.LogError("ExternalLoginInfo is null. OAuth state might be missing or invalid.");
                    return RedirectToAction("Login");
                }

                var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("تسجيل دخول ناجح بـ {Provider}", info.LoginProvider);
                    var Email = info.Principal.FindFirstValue(ClaimTypes.Email);
                    var User = await _userManager.FindByEmailAsync(Email);
                    return RedirectToDashboard(User.RoleType);
                }

                // لو المستخدم مش موجود، ننشئ حساب جديد
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true // External providers are pre-confirmed
                    };
                    var createResult = await _userManager.CreateAsync(user);
                    if (createResult.Succeeded)
                    {
                        await _userManager.AddLoginAsync(user, info);
                        if (string.IsNullOrEmpty(user.RoleType))
                        {
                            return RedirectToAction("CompleteRegistration", new { email });
                        }
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToDashboard(user.RoleType);
                    }
                    else
                    {
                        _logger.LogError("فشل إنشاء المستخدم: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
                        throw new Exception("فشل إنشاء المستخدم");
                    }
                }
                else if (string.IsNullOrEmpty(user.RoleType))
                {
                    return RedirectToAction("CompleteRegistration", new { email });
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToDashboard(user.RoleType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في ExternalLoginCallback: {Message}", ex.Message);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult CompleteRegistration(string email)
        {
            return View(new UserRegistrationModel { Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> CompleteRegistration(UserRegistrationModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            user.RoleType = model.RoleType;
            user.FullName = model.FullName;
            user.Address = model.Address;
            user.CompanyName = model.CompanyName;
            user.LicenseNumber = model.LicenseNumber;
            user.ContactNumber = model.RoleType == "company" ? model.CompanyContactNumber : model.RoleType == "distributor" ? model.DistributorContactNumber : null;
            user.DistributorName = model.DistributorName;
            user.WarehouseAddress = model.WarehouseAddress;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync(model.RoleType))
                {
                    await _roleManager.CreateAsync(new IdentityRole(model.RoleType));
                }
                await _userManager.AddToRoleAsync(user, model.RoleType);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToDashboard(model.RoleType);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        private IActionResult RedirectToDashboard(string roleType)
        {
            string roleTypeLower = roleType?.ToLower() ?? "home";
            _logger.LogInformation("Redirecting to dashboard for role: {RoleType}", roleTypeLower);
            return RedirectToAction(roleTypeLower switch
            {
                "driver" => "DriverShipments",
                "company" => "CompanyDashboard",
                "distributor" => "Dashboard",
                _ => "Index"
            }, roleTypeLower switch
            {
                "driver" => "Driver",
                "company" => "Company",
                "distributor" => "Distributor",
                _ => "Home"
            });
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("✅ تم تسجيل الخروج");
            return RedirectToAction("Login", "Auth");
        }

        

        [HttpGet]
        [AllowAnonymous]
        public IActionResult VerifyOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            var model = new OtpVerificationViewModel 
            { 
                Email = email,
                CanResend = true,
                ResendCountdown = 0
            };
            
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(OtpVerificationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var isValid = await _otpService.ValidateOtpAsync(model.Email, model.OtpCode, "EmailVerification");

            if (isValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);

                    // تسجيل دخول مباشر
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    _logger.LogInformation("✅ تم تأكيد البريد الإلكتروني وتسجيل الدخول: {Email}", model.Email);
                    TempData["SuccessMessage"] = "Your email has been confirmed and you are now logged in!";

                    return RedirectToDashboard(user.RoleType);
                }
            }


            ModelState.AddModelError("OtpCode", "Invalid or expired verification code. Please try again.");
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Email address is required." });
            }

            try
            {
                var canResend = await _otpService.ResendOtpAsync(email, "EmailVerification");
                if (canResend)
                {
                    _logger.LogInformation("📧 تم إعادة إرسال رمز التحقق إلى: {Email}", email);
                    return Json(new { success = true, message = "Verification code has been resent to your email." });
                }
                else
                {
                    return Json(new { success = false, message = "Please wait before requesting another code." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ فشل في إعادة إرسال رمز التحقق: {Email}", email);
                return Json(new { success = false, message = "Failed to resend verification code. Please try again later." });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Invalid email confirmation attempt - missing userId or token");
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Email confirmation attempt for non-existent user: {UserId}", userId);
                return RedirectToAction("Login");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                _logger.LogInformation("✅ تم تأكيد البريد الإلكتروني بنجاح: {Email}", user.Email);
                TempData["SuccessMessage"] = "Your email has been confirmed successfully! You can now log in.";
                return RedirectToAction("Login");
            }
            else
            {
                _logger.LogError("❌ فشل تأكيد البريد الإلكتروني: {Email}, Errors: {Errors}", 
                    user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                TempData["ErrorMessage"] = "Email confirmation failed. The link may have expired.";
                return RedirectToAction("Login");
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Email address is required.";
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                TempData["SuccessMessage"] = "If an account with that email exists, we've sent a confirmation email.";
                return RedirectToAction("Login");
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                TempData["InfoMessage"] = "Your email is already confirmed. You can log in now.";
                return RedirectToAction("Login");
            }

            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "Auth", new { userId = user.Id, token = token }, Request.Scheme);
                
                await _emailService.SendEmailConfirmationAsync(user.Email, confirmationLink);
                _logger.LogInformation("📧 تم إعادة إرسال رابط تأكيد البريد الإلكتروني: {Email}", user.Email);
                
                TempData["SuccessMessage"] = "Confirmation email has been resent. Please check your email.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ فشل في إعادة إرسال البريد الإلكتروني: {Email}", user.Email);
                TempData["ErrorMessage"] = "Failed to send confirmation email. Please try again later.";
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
    }
}

   

