// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using KDC.Main.Data;
using KDC.Main.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace KDC.Main.Areas.Identity.Pages.Account.Manage
{
    public class ResetAuthenticatorModel(
        UserManager<ApplicationUser> _userManager,
        SignInManager<ApplicationUser> _signInManager,
        ILogger<ResetAuthenticatorModel> _logger,
        ApplicationDbContext _applicationDbContext,
        IStringLocalizer<ResetAuthenticatorModel> _localizer
            ) : PageModel
    {

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _userManager.RemoveAuthenticationTokenAsync(user, "Authenticator", "AuthenticatorKey");

            var authenticatorKeys = _applicationDbContext.UserTokens.Where(x => x.UserId == user.Id && x.Name == "AuthenticatorKey");
            _applicationDbContext.RemoveRange(authenticatorKeys);
            await _applicationDbContext.SaveChangesAsync();

            _logger.LogInformation("User with ID '{UserId}' has removed their authenticatior app key.", user.Id);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = _localizer["Your authenticator app has been removed."];

            return RedirectToPage("./TwoFactorAuthentication");
        }
    }
}
