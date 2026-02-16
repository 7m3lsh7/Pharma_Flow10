using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pharmaflow7.Models;
using VM = Pharmaflow7.Models.ViewModels;
using Pharmaflow7.Services;

namespace Pharmaflow7.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly IEmailService _emailService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View(new VM.RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(VM.RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError(string.Empty, "Email is already registered.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                CompanyName = model.CompanyName,
                ContactNumber = model.ContactNumber,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }

            // Assign role
            var role = NormalizeRole(model.Role);
            if (!string.IsNullOrEmpty(role))
            {
                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }

            _logger.LogInformation("User created a new account with email {Email}.", user.Email);

            // Generate email confirmation token
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code }, protocol: Request.Scheme);

            // Send email (implements secure email sending via IEmailService)
            await _emailService.SendEmailAsync(user.Email, "Confirm your email",
                $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");

            return RedirectToAction("RegisterConfirmation", new { email = user.Email });
        }

        [HttpGet]
        public IActionResult RegisterConfirmation(string email)
        {
            ViewData["Email"] = email;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
                return RedirectToAction(nameof(HomeController.Index), "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userId}'.");

            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (!result.Succeeded)
            {
                return View("Error");
            }

            return View("ConfirmEmail");
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new VM.LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(VM.LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Your account has been deactivated. Contact support.");
                return View(model);
            }

            // This will enforce email confirmation due to Identity options
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in.", model.Email);

                // Redirect based on role
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault()?.ToLowerInvariant();
                return role switch
                {
                    "admin" => RedirectToAction("Dashboard", "Admin"),
                    "company" => RedirectToAction("Dashboard", "Company"),
                    "distributor" => RedirectToAction("Dashboard", "Distributor"),
                    "consumer" => RedirectToAction("Index", "Home"),
                    _ => RedirectToLocal(returnUrl)
                };
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return View("Lockout");
            }

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "You must confirm your email to log in.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Lockout()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        private string NormalizeRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role)) return string.Empty;
            var r = role.Trim().ToLowerInvariant();
            return r switch
            {
                "admin" => "Admin",
                "company" => "Company",
                "distributor" => "Distributor",
                "consumer" => "Consumer",
                "driver" => "Driver",
                _ => char.ToUpperInvariant(r[0]) + r.Substring(1)
            };
        }
    }
}
