using Duende.IdentityServer.EntityFramework.DbContexts;
using KDC.Main.Constants;
using KDC.Main.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace KDC.Main.Repositories
{
    public class ClientConfigurationRepository : IClientConfigurationRepository
    {
        private readonly ConfigurationDbContext _configurationDbContext;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ClientConfigurationRepository> _logger;

        public ClientConfigurationRepository(
            ConfigurationDbContext configurationDbContext,
            IMemoryCache cache,
            ILogger<ClientConfigurationRepository> logger)
        {
            _configurationDbContext = configurationDbContext;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<string>> GetRedirectUrisForClientAsync(string clientId)
        {
            var cacheKey = $"{CacheConstants.RedirectUris}_{clientId}";

            if (_cache.TryGetValue(cacheKey, out List<string>? cachedUris))
            {
                return cachedUris ?? new List<string>();
            }

            try
            {
                var redirectUris = await _configurationDbContext.Clients
                    .Where(c => c.ClientId == clientId && c.Enabled)
                    .SelectMany(c => c.RedirectUris)
                    .Select(r => r.RedirectUri)
                    .ToListAsync();

                _cache.Set(cacheKey, redirectUris); 

                return redirectUris;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve redirect URIs for client {ClientId}", clientId);
                return new List<string>();
            }
        }

        public async Task<bool> IsValidRedirectUri(string redirectUri)
        {
            var cacheKey = $"{CacheConstants.RedirectUriValidation}_{redirectUri}";

            if (_cache.TryGetValue(cacheKey, out bool isValidRedirectUri))
            {
                return isValidRedirectUri;
            }

            var result = await _configurationDbContext.Clients
                .SelectMany(c => c.RedirectUris)
                .AnyAsync(r => r.RedirectUri == redirectUri);

            return result;
        }
    }
}
