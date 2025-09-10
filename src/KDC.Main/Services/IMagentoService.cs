using KDC.Main.Data.Models;
using KDC.Main.Models;
using KDC.Main.Models.Magento;

namespace KDC.Main.Services
{
    public interface IMagentoService
    {
        Task<MagentoAuthResult> AuthenticateAsync(string returnUrl, string email, string password);
        Task<MagentoDataResult<StoreConfiguration>?> GetStoreConfigAsync(string? returnUrl = null, string? storeCode = null);
        Task<MagentoRequestResultBase> CreateMagentoAccount(string returnUrl, MagentoAccount magentoAccount);
        Task<MagentoRequestResultBase> InvalidateSession(string? returnUrl, string email, string? storeCode = null);
        Task<MagentoRequestResultBase> IsEmailRegisteredAsync(string returnUrl, string email);
        Task<string?> GetAuthorizationRequest(string storeCode);
        Task<StoreConfiguration?> GetStoreConfigurationByCodeAsync(string storeCode);
    }
}
