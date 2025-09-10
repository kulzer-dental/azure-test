// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KDC.Main.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace KDC.Main.Areas.Identity.Pages.Account
{
    public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginWith2faModel> _logger;

        public LoginWith2faModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginWith2faModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        public IList<string> Providers { get; set; }

        public bool RememberMe { get; set; }
        public string ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
        {
            var user = await GetAndValidateUserAsync();

            Providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
            Providers = Providers.OrderBy(x => x).ToList();

            ReturnUrl = returnUrl;
            RememberMe = rememberMe;
            if (Providers.Count == 1)
            {
                return RedirectToProviderPage(Providers.First(), returnUrl, rememberMe);
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string provider, string returnUrl = null, bool rememberMe = false)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            returnUrl = returnUrl ?? Url.Content("~/");
            await GetAndValidateUserAsync();

            return RedirectToProviderPage(provider, returnUrl, rememberMe);
        }
        
        private async Task<ApplicationUser> GetAndValidateUserAsync()
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }
            return user;
        }

        private IActionResult RedirectToProviderPage(string provider, string returnUrl, bool rememberMe)
        {
            return RedirectToPage(
                provider == "Authenticator" ? "./LoginWith2faAuthenticator" : "./LoginWith2faEmail",
                new { ReturnUrl = returnUrl, RememberMe = rememberMe }
            );
        }
    }
}
