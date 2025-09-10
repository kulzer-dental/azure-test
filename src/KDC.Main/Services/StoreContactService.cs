using KDC.Main.Config;
using Microsoft.Extensions.Options;

namespace KDC.Main.Services
{
    public class StoreContactService: IStoreContactService
    {
        private readonly StoresContactsConfig _config;

        public StoreContactService(IOptions<StoresContactsConfig> config)
        {
            _config = config.Value;
        }

        public string GetSupportEmail(string storeCode)
        {
            var storeContact = GetStoreContacts(storeCode);
            return storeContact?.SupportEmail ?? "support@kulzer.com";
        }       

        public StoreContact? GetStoreContacts(string storeCode)
        {
            if (string.IsNullOrEmpty(storeCode))
                return null;

            return _config.Stores.Values
                .FirstOrDefault(store => store.Id.Equals(storeCode, StringComparison.OrdinalIgnoreCase));
        }
    }
}
