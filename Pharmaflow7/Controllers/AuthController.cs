using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Pharmaflow7.Models;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace Pharmaflow7.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
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
            if (string.IsNullOrEmpty(model.Password))
                ModelState.AddModelError("Password", "Password is required.");
            if (model.Password.Length < 8 || !model.Password.Any(char.IsUpper) || !model.Password.Any(char.IsDigit))
                ModelState.AddModelError("Password", "Password must be at least 8 characters, with an uppercase letter and a number.");
            if (string.IsNullOrEmpty(model.RoleType))
                ModelState.AddModelError("RoleType", "User type is required.");

            switch (model.RoleType)
            {
                case "consumer":
                    if (string.IsNullOrEmpty(model.FullName))
                        ModelState.AddModelError("FullName", "Full Name is required for consumers.");
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
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToDashboard(model.RoleType);
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
                        Email = email
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
                "consumer" => "ConsumerDashboard",
                "company" => "CompanyDashboard",
                "distributor" => "Dashboard",
                _ => "Index"
            }, roleTypeLower switch
            {
                "consumer" => "Consumer",
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
        public IActionResult AccessDenied(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
    }
}

   

