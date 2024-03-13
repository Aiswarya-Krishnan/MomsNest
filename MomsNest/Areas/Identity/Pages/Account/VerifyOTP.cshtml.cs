using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MomsNest.Models;

namespace MomsNest.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class VerifyOTPModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<VerifyOTPModel> _logger;

        public VerifyOTPModel(UserManager<ApplicationUser> userManager, ILogger<VerifyOTPModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public TwoStepModel TwoStepModel { get; set; }

        public string Email { get; set; }
        public string ReturnUrl { get; set; }
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/Index");
            }

            Email = email;
            ReturnUrl = returnUrl;

            // Clear error message
            ErrorMessage = null;

            return Page();
        }

        [HttpPost]
        public async Task<IActionResult> OnPostAsync(string email, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid user.");
                return Page();
            }

            var isTokenValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", TwoStepModel.TwoFactorCode);
            if (!isTokenValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid OTP.");
                ErrorMessage = "Invalid OTP.";
                ModelState.Remove("TwoStepModel.TwoFactorCode");
                return RedirectToPage(new { email = email, returnUrl = returnUrl, errorMessage = ErrorMessage });
            }
            
              
            // OTP is valid, disable Two-Factor Authentication
            await _userManager.SetTwoFactorEnabledAsync(user, false);

            _logger.LogInformation("OTP successfully verified for user: {Email}", email);

            // Redirect user to the login page
            return Redirect("~/Identity/Account/Login?returnUrl=" + ReturnUrl);
        }
    }
}
