using Serilog;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;

internal static partial class HostingExtensions
{
    private static IEnumerable<IdentityResource> IdentityResources =>
            [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            ];

    private static IEnumerable<ApiScope> ApiScopes =>
        [
            new ApiScope("scope1"),
            new ApiScope("scope2"),
        ];

    private static IEnumerable<Client> Clients =>
        [
            // m2m client credentials flow client
            new Client
            {
                ClientId = "m2m.client",
                ClientName = "Client Credentials Client",

                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                AllowedScopes = { "scope1" }
            },
            new Client {
                     ClientId = "m2shop-belvg",
                     ClientName = "Kulzer Magento 2 BelVG DEV",
                     AllowedGrantTypes = GrantTypes.Code,
                     RequirePkce = false,
                     RedirectUris = {
                        "https://kulzer-auth.s2.belvgdev.com/mooauth/actions/ReadAuthorizationResponse",
                        "https://kulzer-auth-us.s2.belvgdev.com/mooauth/actions/ReadAuthorizationResponse",
                        "https://kulzer-auth-dk.s2.belvgdev.com/mooauth/actions/ReadAuthorizationResponse",
                        "https://kulzer-auth-fi.s2.belvgdev.com/mooauth/actions/ReadAuthorizationResponse",
                        "https://kulzer-auth-fr.s2.belvgdev.com/mooauth/actions/ReadAuthorizationResponse",
                        "https://kulzer-auth-au.s2.belvgdev.com/mooauth/actions/ReadAuthorizationResponse",
                     },
                     ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },
                     AllowedScopes = { "openid", "profile" },
            },
            new Client {
                    ClientId = "m2shop-us-belvg",
                    ClientName = "Kulzer Magento 2 US BelVG DEV",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = false,
                    RedirectUris = { "https://kulzer-auth-us.s2.belvgdev.com/mooauth/actions/ReadAuthorizationResponse" },
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },
                    AllowedScopes = { "openid", "profile" },
            },
            // interactive client using code flow + pkce
            new Client
            {
                ClientId = "interactive",
                ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                AllowedGrantTypes = GrantTypes.Code,

                RedirectUris = { "https://localhost:44300/signin-oidc" },
                FrontChannelLogoutUri = "https://localhost:44300/signout-oidc",
                PostLogoutRedirectUris = { "https://localhost:44300/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "scope2" }
            },
        ];

    /// <summary>
    /// Seeds initial config for identity server 
    /// </summary>
    /// <param name="app"></param>
    public static void EnsureSeedIdentityServerConfiguration(this WebApplication app)
    {
        using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
        using var transaction = context.Database.BeginTransaction();

        try
        {

            if (!context.Clients.Any())
            {
                foreach (var client in Clients)
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.IdentityResources.Any())
            {
                foreach (var resource in IdentityResources)
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.ApiScopes.Any())
            {
                foreach (var resource in ApiScopes)
                {
                    context.ApiScopes.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

            // Commit transaction if all commands succeed, transaction will auto-rollback
            // when disposed if either commands fails
            transaction.Commit();
            Log.Debug($"Seeding Identity Server Configuration.");
        }
        catch (Exception)
        {
            transaction.Rollback();
            Log.Error($"Seeding Identity Server Configuration failed.");
        }
    }

}
