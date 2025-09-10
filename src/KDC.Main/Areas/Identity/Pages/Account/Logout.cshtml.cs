// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Duende.IdentityServer.Services;
using KDC.Main.Data.Models;
using KDC.Main.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KDC.Main.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IMagentoService _magentoService;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, 
            ILogger<LogoutModel> logger,
            IIdentityServerInteractionService interaction,
            IMagentoService magentoService)
        {
            _signInManager = signInManager;
            _logger = logger;
            _interaction = interaction;
            _magentoService = magentoService;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await LogoutAsync(returnUrl);
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnGet(string logoutId, string returnUrl = null)
        {
            await LogoutAsync(returnUrl);

           //I don't have the client in the database
           var logoutRequest = await _interaction.GetLogoutContextAsync(logoutId);

            var redirectUri = logoutRequest?.PostLogoutRedirectUri ?? "https://localhost:5003/home";

             if (string.IsNullOrEmpty(returnUrl) == false)
             {
                 return Redirect(returnUrl);
             }

             return Redirect(redirectUri);
        }


        private async Task LogoutAsync(string returnUrl)
        {
            var currentUser = await _signInManager.UserManager.GetUserAsync(User);

            if (currentUser != null)
            {
                // Log out user from Magento
                await InvalidateMagentoSessionAsync(currentUser.Email, returnUrl, currentUser.StoreCode);
            }

            await _signInManager.SignOutAsync();
        }

        private async Task InvalidateMagentoSessionAsync(string email, string returnUrl, string storeCode)
        {
            try
            {
                var response = await _magentoService.InvalidateSession(returnUrl, email, storeCode);

                if (response?.Success == true)
                {
                    _logger.LogInformation("Successfully invalidated Magento session for user: {Email}", email);
                }                
                else
                {
                    _logger.LogWarning($"Failed to invalidate Magento session for user {email}: {response?.ErrorMessage ?? "Unknown error"}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while invalidating Magento session for user: {Email}", email);
            }
        }
    }
}
