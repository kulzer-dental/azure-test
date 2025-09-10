// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using KDC.Main.Config;
using KDC.Main.Data.Models;
using KDC.Main.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace KDC.Main.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IExtendedEmailSender _emailSender;
        private readonly IMagentoService _magentoService;
        private readonly IUserMigrationService _userMigrationService;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager,
            IExtendedEmailSender emailSender,
            IMagentoService magentoService,
            IUserMigrationService userMigrationService)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _magentoService = magentoService;
            _userMigrationService = userMigrationService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "Your email address is required")]
            [Display(Name = "Your email")]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            bool shouldProcceedWithReset = true;
            var returnUrl = Request.Query["returnUrl"].FirstOrDefault();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    if (Helpers.UrlHelper.IsRedirectUrlNullOrDefault(returnUrl) == false)
                    {
                        var isUserExistedInMagento = await _magentoService.IsEmailRegisteredAsync(returnUrl, Input.Email);

                        string password = string.Concat(Guid.NewGuid().ToString()
                    .Select(c => char.IsLetter(c) && Random.Shared.Next(2) == 0 ? char.ToUpper(c) : c));

                        shouldProcceedWithReset = await _userMigrationService.MigrateUserFromMagentoAsync(
                            Input.Email,
                            password,
                            returnUrl,
                            isEmailConfirmationRequired: 0,
                            shouldBlockUserRecoverPassword: true);

                        if (shouldProcceedWithReset)
                        {
                            user = await _userManager.FindByEmailAsync(Input.Email);
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "An error occurred, please try again.");
                            return Page();
                        }
                    }
                    
                    if(user != null)
                    {
                        await ResetPassword(user);
                    }
                }
                else
                {
                    await ResetPassword(user);
                }             
            }

            return RedirectToPage("./ForgotPasswordConfirmation");
        }

        private async Task ResetPassword(ApplicationUser user)
        {
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code, email = user.Email },
                protocol: Request.Scheme);


            var userClaims = await _userManager.GetClaimsAsync(user);
            var cultureClaim = userClaims.FirstOrDefault(c => c.Type == AppClaimTypes.Culture);
            var userCulture = cultureClaim?.Value ?? "en";

           _= _emailSender.SendResetPasswordEmailAsync(Input.Email, resetUrl: HtmlEncoder.Default.Encode(callbackUrl), 
                storeCode: user.StoreCode, userCulture: userCulture);
        }
    }
}
