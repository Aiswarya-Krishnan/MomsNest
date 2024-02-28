using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MomsNest.Models;

namespace MomsNest.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class VerifyOTPModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public VerifyOTPModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public TwoStepModel TwoStepModel { get; set; }  // Corrected property name

        public string Email { get; set; }
        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/Index");
            }

            Email = email;
            ReturnUrl = returnUrl;

            return Page();
        }
        [HttpPost]
        public async Task<IActionResult> OnPostAsync(string email, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");


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
                return Page();
            }

            // OTP is valid, disable Two-Factor Authentication
            await _userManager.SetTwoFactorEnabledAsync(user, false);

            // Redirect user to the login page
            return RedirectToPage("/Account/Login");
        }

    }
}