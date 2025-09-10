using KDC.Main.Config;
using KDC.Main.Data.Models;
using KDC.Main.Helpers;
using Microsoft.AspNetCore.Identity;
using Sprache;
using System.Globalization;
using System.Security.Claims;

namespace KDC.Main.Services
{
    public class UserMigrationService : IUserMigrationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IMagentoService _magentoService;
        private readonly ILogger<UserMigrationService> _logger;

        public UserMigrationService(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            IMagentoService magentoService,
            ILogger<UserMigrationService> logger)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = (IUserEmailStore<ApplicationUser>)userStore;
            _magentoService = magentoService;
            _logger = logger;
        }

        public async Task<bool>   MigrateUserFromMagentoAsync(string email, string password, string returnUrl, int isEmailConfirmationRequired, bool shouldBlockUserRecoverPassword = false)
        {
            try
            {
                var storeCode = UrlHelper.ExtractRedirectUri(returnUrl, "store_code");

                if (storeCode == null)
                {
                    _logger.LogWarning("Store code not found in return URL: {ReturnUrl}", returnUrl);
                    return false;
                }

                var user = UserFactory.CreateUser(storeCode);

                var culture = await GetUserCulture(returnUrl, storeCode);

                user.EmailConfirmed = isEmailConfirmationRequired != 1;
                user.LockoutEnabled = shouldBlockUserRecoverPassword;

                await _userStore.SetUserNameAsync(user, email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    var newCultureClaim = new Claim(AppClaimTypes.Culture, culture.Name);
                    await _userManager.AddClaimAsync(user, newCultureClaim);

                    if (shouldBlockUserRecoverPassword)
                    {
                        var lockoutResult = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                        _logger.LogInformation("User {Email} was blocked {result}, {errors}", email, lockoutResult?.Succeeded, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }

                    _logger.LogInformation("User {Email} migrated from Magento to IdentityServer", email);
                }
                else
                {
                    _logger.LogWarning("User {Email} could not be migrated from Magento to IdentityServer. Errors: {Errors}",
                        email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error migrating user {Email} from Magento", email);
                return false;
            }
        }

        public async Task<ApplicationUser?> GetOrCreateUserFromMagentoAsync(string email, string password, string returnUrl)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            var storeCode = UrlHelper.ExtractRedirectUri(returnUrl, "store_code");
            bool allowCrossStoreLogin = ShouldAllowCrossStoreLogin(existingUser, storeCode);

            if (existingUser != null && allowCrossStoreLogin == false)
            {
                _logger.LogDebug("Returning existing user {Email}", email);
                return existingUser;
            }

            if (existingUser != null && allowCrossStoreLogin)
            {
                return await ValidateCrossStoreRegistrationAsync(email, returnUrl, storeCode, existingUser);
            }

            return await HandleMigrationAsync(returnUrl, email, password);
        }

        private async Task<int> GetEmailConfirmationRequirementAsync(string returnUrl)
        {
            try
            {
                var storeCode = UrlHelper.ExtractRedirectUri(returnUrl, "store_code");
                var emailConfirmationEnabled = UrlHelper.ExtractRedirectUri(returnUrl, "email_confirmation_enabled");

                if (int.TryParse(emailConfirmationEnabled, out var enabled))
                {
                    var storesConfiguration = await _magentoService.GetStoreConfigAsync(returnUrl);
                    var hasMatchingSettings = storesConfiguration?.Data?.Any(data =>
                        data.Code == storeCode && data.ExtensionAttributes.EmailConfirmationEnabled == enabled);

                    return hasMatchingSettings == true ? enabled : 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error retrieving email confirmation requirement from URL");
            }

            return 0;
        }

        private bool ShouldAllowCrossStoreLogin(ApplicationUser? existingUser, string? storeCode)
        {
            if (existingUser == null)
            {
                _logger.LogDebug("Login to another store denied: User is null");
                return false;
            }

            if (string.IsNullOrEmpty(storeCode))
            {
                _logger.LogWarning("Store code not found in return URL");
                return false;
            }

            var isAllowed = string.Equals(storeCode, existingUser.StoreCode, StringComparison.OrdinalIgnoreCase) == false;

            return isAllowed;
        }

        private async Task<ApplicationUser?> HandleMigrationAsync(string returnUrl, string email, string password)
        {
            var magentoAuth = await _magentoService.AuthenticateAsync(returnUrl, email, password);
            if (magentoAuth.Success == false)
            {
                return null;
            }

            var isEmailConfirmationRequired = await GetEmailConfirmationRequirementAsync(returnUrl);

            var migrationSuccess = await MigrateUserFromMagentoAsync(email, password, returnUrl, isEmailConfirmationRequired);
            return migrationSuccess ? await _userManager.FindByEmailAsync(email) : null;
        }

        private async Task<ApplicationUser?> ValidateCrossStoreRegistrationAsync(string email, string returnUrl, string? storeCode, ApplicationUser existingUser)
        {
            var response = await _magentoService.IsEmailRegisteredAsync(returnUrl, email);
            if (response.Success == false)
            {
                _logger.LogWarning("Email {Email} is not registered in Magento store code {StoreCode}", email, storeCode);
                return null;
            }

            return existingUser;
        }

        private async Task<CultureInfo> GetUserCulture(string? returnUrl, string? storeCode)
        {
            var storeConfig = await _magentoService.GetStoreConfigAsync(returnUrl, storeCode);

            var language = storeConfig?.Data?.FirstOrDefault()?.Locale ?? "en";
            language = language?.Split('_')[0];

            try
            {
                var culture = new CultureInfo(language);

                var availableCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
                var isValidCulture = availableCultures.Any(c =>
                   string.Equals(c.Name, language, StringComparison.OrdinalIgnoreCase) &&
                   !string.IsNullOrEmpty(c.Name));

                if (!isValidCulture)
                {
                    _logger.LogWarning("Culture {Language} is not a valid specific culture, defaulting to en-US", language);
                    return new CultureInfo("en");
                }

                return culture;
            }
            catch (CultureNotFoundException)
            {
                _logger.LogWarning("Culture {Language} not found, defaulting to en-US", language);
                return new CultureInfo("en");
            }
        }
    }
}
