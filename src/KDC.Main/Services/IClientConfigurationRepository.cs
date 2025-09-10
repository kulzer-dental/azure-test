namespace KDC.Main.Services
{
    public interface IClientConfigurationRepository
    {
        Task<List<string>> GetRedirectUrisForClientAsync(string clientId);
        Task<bool> IsValidRedirectUri(string redirectUri);
    }
}
