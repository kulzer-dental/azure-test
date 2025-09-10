using KDC.Main.Helpers;

namespace KDC.Main.Services
{
    public class StoreConfigurationService: IStoreConfigurationService
    {
        private readonly IMagentoService _magentoService;
        private readonly ILogger<StoreConfigurationService> _logger;

        public StoreConfigurationService(IMagentoService magentoService, ILogger<StoreConfigurationService> logger  )
        {
            _magentoService = magentoService;
            _logger = logger;

        }

        public async Task<bool> IsSelfRegistrationAllowed(string returnUrl)
        {
            bool shouldSefRegister = false;

            var storeCode = UrlHelper.ExtractRedirectUri(returnUrl, "store_code");
            
            if(string.IsNullOrEmpty(storeCode))
            {
                _logger.LogWarning("Store code not found in return URL: {ReturnUrl}", returnUrl);
                return false;
            }

            var storeConfiguration = await _magentoService.GetStoreConfigurationByCodeAsync(storeCode);

            if (storeConfiguration != null)
            {
                shouldSefRegister = storeConfiguration?.ExtensionAttributes?.SelfServiceRegistrationEnabled == 1;
            }

#if DEBUG
            //TODO remove this on the production environment
            //this is only set cuz in Germany self registrion is false
            shouldSefRegister = true;
#endif

            return shouldSefRegister;
        }

        public async Task<int> IsStoreEnabledEmailConfirmation(string returnUrl, string? storeCode = null)
        {
            try
            {
                storeCode = storeCode ?? UrlHelper.ExtractRedirectUri(returnUrl, "store_code");
                string? email_confirmation_enabled = UrlHelper.ExtractRedirectUri(returnUrl, "email_confirmation_enabled");
                
                if (email_confirmation_enabled == null)
                {
                    _logger.LogInformation("Email confirmation enabled parameter is missing.");
                    return 0;
                }

                var emailConfirmationEnabled = Int32.Parse(email_confirmation_enabled);

                var storesConfiguration = await _magentoService.GetStoreConfigAsync(returnUrl);
                var hasMatchingSettings = storesConfiguration?.Data?.Any(Data => Data.Code == storeCode && Data.ExtensionAttributes.EmailConfirmationEnabled == emailConfirmationEnabled);

                return hasMatchingSettings == true ? emailConfirmationEnabled : 0;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception during retrieve from URL parameters ex {ex}");
            }

            return 0;
        }
    }
}
