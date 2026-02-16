using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pharmaflow7.Data;
using PharmaFlow.Models.ViewModels;
using Pharmaflow7.Models;
using System.Security.Cryptography;
using System.Text;

namespace PharmaFlow.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            AppDbContext context,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var companies = await _userManager.GetUsersInRoleAsync("company");
            var distributors = await _userManager.GetUsersInRoleAsync("distributor");
            var consumers = await _userManager.GetUsersInRoleAsync("consumer");

            var viewModel = new AdminDashboardViewModel
            {
                TotalCompanies = companies.Count,
                ActiveCompanies = companies.Count(c => c.IsActive),
                TotalDistributors = distributors.Count,
                TotalConsumers = consumers.Count,
                TotalDrugs = 0, // Update with actual drug count from your database
                RecentCompanies = companies
                    .OrderByDescending(c => c.CreatedDate)
                    .Take(10)
                    .Select(c => new CompanyListItemViewModel
                    {
                        Id = c.Id,
                        CompanyName = c.CompanyName ?? "N/A",
                        Email = c.Email ?? "",
                        IsActive = c.IsActive,
                        IsVerified = c.IsVerified,
                        CreatedDate = c.CreatedDate
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        // GET: Admin/Companies
        public async Task<IActionResult> Companies()
        {
            var companies = await _userManager.GetUsersInRoleAsync("company");
            var viewModel = companies.Select(c => new CompanyListItemViewModel
            {
                Id = c.Id,
                CompanyName = c.CompanyName ?? "N/A",
                Email = c.Email ?? "",
                IsActive = c.IsActive,
                IsVerified = c.IsVerified,
                CreatedDate = c.CreatedDate
            }).ToList();

            return View(viewModel);
        }

        // GET: Admin/CreateCompany
        public IActionResult CreateCompany()
        {
            return View();
        }

        // POST: Admin/CreateCompany
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCompany(CreateCompanyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            // Generate secure random password
            var password = GenerateSecurePassword();

            // Create new company user
            var company = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                CompanyName = model.CompanyName,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true,
                IsVerified = model.VerifyImmediately,
                IsActive = true,
                UserType = "company",
                CreatedByAdminId = _userManager.GetUserId(User),
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(company, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(company, "company");

                // In production, send email with credentials
                // For now, store in TempData to display
                TempData["SuccessMessage"] = $"Company account created successfully!";
                TempData["CompanyEmail"] = model.Email;
                TempData["CompanyPassword"] = password;

                _logger.LogInformation($"Admin created company account: {model.Email}");

                return RedirectToAction(nameof(CompanyCreated));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: Admin/CompanyCreated
        public IActionResult CompanyCreated()
        {
            if (TempData["CompanyEmail"] == null)
            {
                return RedirectToAction(nameof(Dashboard));
            }

            return View();
        }

        // GET: Admin/EditCompany/5
        public async Task<IActionResult> EditCompany(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var company = await _userManager.FindByIdAsync(id);
            if (company == null || !await _userManager.IsInRoleAsync(company, "company"))
            {
                return NotFound();
            }

            var viewModel = new EditCompanyViewModel
            {
                Id = company.Id,
                CompanyName = company.CompanyName ?? "",
                Email = company.Email ?? "",
                FullName = company.FullName,
                PhoneNumber = company.PhoneNumber,
                IsActive = company.IsActive,
                IsVerified = company.IsVerified
            };

            return View(viewModel);
        }

        // POST: Admin/EditCompany/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCompany(string id, EditCompanyViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var company = await _userManager.FindByIdAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            company.CompanyName = model.CompanyName;
            company.FullName = model.FullName;
            company.PhoneNumber = model.PhoneNumber;
            company.IsActive = model.IsActive;
            company.IsVerified = model.IsVerified;
            company.LastModifiedDate = DateTime.UtcNow;

            // Update email if changed
            if (company.Email != model.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(company, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
                await _userManager.SetUserNameAsync(company, model.Email);
            }

            var result = await _userManager.UpdateAsync(company);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Company updated successfully!";
                return RedirectToAction(nameof(Companies));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // POST: Admin/ToggleCompanyStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCompanyStatus(string id)
        {
            var company = await _userManager.FindByIdAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            company.IsActive = !company.IsActive;
            company.LastModifiedDate = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(company);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Company {(company.IsActive ? "activated" : "deactivated")} successfully!";
            }

            return RedirectToAction(nameof(Companies));
        }

        // POST: Admin/DeleteCompany/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCompany(string id)
        {
            var company = await _userManager.FindByIdAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(company);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Company deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete company.";
            }

            return RedirectToAction(nameof(Companies));
        }

        // Helper method to generate secure password
        private string GenerateSecurePassword()
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            var random = new Random();
            var password = new StringBuilder();

            // Ensure password has at least one of each required character type
            password.Append(validChars[random.Next(26, 52)]); // Uppercase
            password.Append(validChars[random.Next(0, 26)]); // Lowercase
            password.Append(validChars[random.Next(52, 62)]); // Digit
            password.Append(validChars[random.Next(62, validChars.Length)]); // Special char

            // Fill the rest randomly
            for (int i = 4; i < 12; i++)
            {
                password.Append(validChars[random.Next(validChars.Length)]);
            }

            // Shuffle the password
            return new string(password.ToString().OrderBy(x => random.Next()).ToArray());
        }
    }
}
