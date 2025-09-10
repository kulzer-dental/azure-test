// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.Services;
using KDC.Main.Data.Models;
using KDC.Main.Helpers;
using KDC.Main.Models;
using KDC.Main.Services;
using Localization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace KDC.Main.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IStringLocalizer<Shared> _localizer;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMagentoService _magentoService;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ConfigurationDbContext _configurationDbContext;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IUserMigrationService _userMigrationService;
        private readonly IStoreConfigurationService _storeConfigurationService;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger,
            IStringLocalizer<Shared> localizer,
            UserManager<ApplicationUser> userManager,
            IMagentoService magentoService,
            IUserStore<ApplicationUser> userStore,
            ConfigurationDbContext configurationDbContext,
            IIdentityServerInteractionService interaction,
            IUserMigrationService userMigrationService,
            IStoreConfigurationService storeConfigurationService
            )
        {
            _signInManager = signInManager;
            _logger = logger;
            _localizer = localizer;
            _userManager = userManager;
            _magentoService = magentoService;
            _userStore = userStore;
            _emailStore = (IUserEmailStore<ApplicationUser>)userStore;
            _configurationDbContext = configurationDbContext;
            _interaction = interaction;
            _userMigrationService = userMigrationService;
            _storeConfigurationService = storeConfigurationService;
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
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

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

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "Your password is required")]
            [Display(Name = "Password")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
               
                var user = await _userMigrationService.GetOrCreateUserFromMagentoAsync(Input.Email, Input.Password, returnUrl);

                if (user != null)
                {
                    // User exists or was successfully created/migrated, now attempt login
                    return await LoginBasedOnIdentityServerUser(returnUrl, user.StoreCode);
                }
                else
                {
                    // Authentication failed (user doesn't exist in Magento or other error)
                    ModelState.AddModelError(string.Empty, _localizer["Invalid login attempt."]);
                }               
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private async Task<ActionResult> LoginBasedOnIdentityServerUser(string returnUrl, string storeCode = null)
        {
            // Sign in the user using IdentityServer
            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                if (UrlHelper.IsRedirectUrlNullOrDefault(returnUrl) == true)
                {
                    var returnUrlAuthorization = await _magentoService.GetAuthorizationRequest(storeCode);
                    if (returnUrlAuthorization == null)
                    {
                        _logger.LogError("Return to /Index returnUrlAuthorization is null");
                        return RedirectToPage("./Index");
                    }

                    return Redirect(returnUrlAuthorization);
                }

                return LocalRedirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, _localizer["Invalid login attempt."]);
                return Page();
            }
        }
       

        private async Task LogError(string returnUrl)
        {
            var errorId = UrlHelper.ExtractRedirectUri(returnUrl, "errorId");

            if (errorId != null)
            {
                var errorMessage = await _interaction.GetErrorContextAsync(errorId);
                _logger.LogInformation(errorMessage.Error);

            }
        }
    }
}
