namespace KDC.Main.Services
{
    public interface IStoreConfigurationService
    {
        Task<bool> IsSelfRegistrationAllowed(string returnUrl);
        Task<int> IsStoreEnabledEmailConfirmation(string returnUrl, string? storeCode = null);
    }
}
