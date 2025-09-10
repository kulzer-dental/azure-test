// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using KDC.Main.Config;
using KDC.Main.Data.Models;
using KDC.Main.Helpers;
using KDC.Main.Services;
using Localization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace KDC.Main.Areas.Identity.Pages.Account
{
    public class LoginWith2faEmailModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginWith2faEmailModel> _logger;
        private readonly IExtendedEmailSender _emailSender;
        private readonly IStringLocalizer<Shared> _sharedLocalizer;
        private readonly IMagentoService _magentoService;

        public LoginWith2faEmailModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<LoginWith2faEmailModel> logger,
            IEmailService emailService,
            IExtendedEmailSender emailSender,
            IStringLocalizer<Shared> sharedLocalizer,
            IMagentoService magentoService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _sharedLocalizer = sharedLocalizer;
            _magentoService = magentoService;
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
        public bool RememberMe { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

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
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Code")]
            public string TwoFactorCode { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember this machine")]
            public bool RememberMachine { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

            var userClaims = await _userManager.GetClaimsAsync(user);
            var cultureClaim = userClaims.FirstOrDefault(c => c.Type == AppClaimTypes.Culture);

            var defaultCulture = Thread.CurrentThread?.CurrentUICulture?.Name ?? "en";
            var userCulture = cultureClaim?.Value ?? defaultCulture;

             _= _emailSender.SendTwoFactorEmailAsync(
                user.Email,
                token,
                user.StoreCode,
                userCulture);

            ReturnUrl = returnUrl;
            RememberMe = rememberMe;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var twoFactorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorSignInAsync("Email", twoFactorCode, rememberMe, Input.RememberMachine);

            if (result.Succeeded)
            {
                if (UrlHelper.IsRedirectUrlNullOrDefault(returnUrl) == true)
                {
                    var returnUrlAuthorization = await _magentoService.GetAuthorizationRequest(user.StoreCode);
                    if (returnUrlAuthorization == null)
                    {
                        _logger.LogError("Return to /Index returnUrlAuthorization is null");
                        return RedirectToPage("./Index");
                    }

                    return Redirect(returnUrlAuthorization);
                }

                return LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return RedirectToPage("./Lockout");
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
                ModelState.AddModelError(string.Empty, _sharedLocalizer["Invalid code."]);
                return Page();
            }
        }
    }
}
