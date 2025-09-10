// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using KDC.Main.Config;
using KDC.Main.Data.Models;
using KDC.Main.Helpers;
using KDC.Main.Models.Magento;
using KDC.Main.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace KDC.Main.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailService _emailService;
        private readonly IStoreConfigurationService _storeRegistrationService;
        private readonly IMagentoService _magentoService;
        private readonly IExtendedEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailService emailService,
            IStoreConfigurationService storeConfigurationService,
            IMagentoService magentoService,
            IExtendedEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailService = emailService;
            _storeRegistrationService = storeConfigurationService;
            _magentoService = magentoService;
            _emailSender = emailSender;
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
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

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
            [EmailAddress]
            [Display(Name = "Your email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "The password is required")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "Your first name is required")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Your last name is required")]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }           
        }


        public async Task OnGetAsync(string returnUrl = null, bool? self_service_registration_enabled = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                var uri = new Uri("https://dummy.local" + returnUrl); // dummy base to parse
                var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
                if (queryParams.TryGetValue("self_service_registration_enabled", out var value))
                {
                    var SelfServiceRegistrationEnabled = value == "1" || value == "true";
                }
            }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var selfServiceRegistrationEnabled = await _storeRegistrationService.IsSelfRegistrationAllowed(returnUrl);

                if (selfServiceRegistrationEnabled == true)
                {
                    var applicationUser = await _userManager.FindByEmailAsync(Input.Email);
                   
                    if(applicationUser == null)
                    {
                        var result = await PerformSelfRegistrationAsync(returnUrl);
                        return result;
                    }

                    TempData["ErrorMessage"] = "An account with this email adress already exists!";                   
                }
                else
                {
                    var storeCode = UrlHelper.ExtractRedirectUri(returnUrl, "store_code");
                    _logger.LogError($"Store self registration is set to false store_code {storeCode}");
                }
            }

            return Page();
        }        

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }

        public async Task<ActionResult> PerformSelfRegistrationAsync(string returnUrl)
        {
            var magentoAccount = new MagentoAccount
            {
                Customer = new MagentoAccount.CustomerInfo
                {
                    Email = Input.Email,
                    FirstName = Input.Email,
                    LastName = Input.Email
                },
                Password = Input.Password
            };

            var magentoApiResult = await _magentoService.CreateMagentoAccount(returnUrl, magentoAccount);

            if (magentoApiResult.Success == true)
            {
                var storeCode = UrlHelper.ExtractRedirectUri(returnUrl, "store_code");
                var user = UserFactory.CreateUser(storeCode);

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    var userClaims = await _userManager.GetClaimsAsync(user);
                    var cultureClaim = userClaims.FirstOrDefault(c => c.Type == AppClaimTypes.Culture);

                    var defaultCulture = Thread.CurrentThread?.CurrentUICulture?.Name ?? "en"; 
                    var userCulture = cultureClaim?.Value ?? defaultCulture;

                   _= _emailSender.SendConfirmationEmailAsync(email: Input.Email, confirmUrl: HtmlEncoder.Default.Encode(callbackUrl),
                        storeCode: user.StoreCode, userCulture: userCulture);

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                TempData["ErrorMessage"] = magentoApiResult.ErrorMessage;
            }

            return Page();
        }
    }
}
