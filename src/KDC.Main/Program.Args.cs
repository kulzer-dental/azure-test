using Duende.IdentityServer.EntityFramework.DbContexts;
using KDC.Main.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

internal static partial class HostingExtensions
{
    public static WebApplication ExecuteArgs(this WebApplication app, string[] args)
    {
        var environment = DotNetEnv.Env.GetString("ASPNETCORE_ENVIRONMENT");

        if (
            environment != "Development" ||
            (!args.Contains("/drop") && !args.Contains("/migrate") && !args.Contains("/seed"))
        )
        {
            return app;
        }

        // This drops the database in case of complete restart
        if (args.Contains("/drop"))
        {
            app.DropDatabase();
        }

        // This applies ef migrations only
        if (args.Contains("/migrate"))
        {
            app.EnsureDatabaseMigrations();
        }

        // This applies migrations and seeds the database
        if (args.Contains("/seed"))
        {
            app.EnsureDatabaseMigrations();
            app.EnsureSeedRoles();
            app.EnsureSeedUsers();
            app.EnsureSeedIdentityServerConfiguration();
        }

        // Just kill the app
        Environment.Exit(0);
        return app;

    }

    /// <summary>
    /// Drops the database (DELETE ALL!)
    /// </summary>
    /// <param name="app"></param>
    public static void DropDatabase(this WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            Log.Information("Dropping database if exists...");
            var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            applicationDbContext.Database.EnsureDeleted();
            Log.Information("Done dropping database.");
        }
    }

    /// <summary>
    /// Ensure latest migrations are applied
    /// </summary>
    /// <param name="app"></param>
    public static void EnsureDatabaseMigrations(this WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            Log.Information("Updating database...");

            var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            applicationDbContext.Database.Migrate();

            var persistedGrantDbContext = scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>();
            persistedGrantDbContext.Database.Migrate();

            var configurationDbContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            configurationDbContext.Database.Migrate();

            var client = configurationDbContext.Clients
                .Include(c => c.RedirectUris)
                .Include(c => c.AllowedCorsOrigins)
                .SingleOrDefault(c => c.ClientId == "m2shop-belvg");

            if (client != null)
                    {
                        // Your list of RedirectUris to ensure
                        var redirectUrisToAdd = new List<string>
                        {
                            "https://kulzer-auth-us.s2.belvgdev.com/mooauth/actions/ReadAuthorizationResponse",
                            "https://kulzer-auth-dk.s2.belvgdev.com/mooauth/actions/ReadAuthorizationResponse",
                            "https://kulzer-auth-fi.s2.belvgdev.com/mooauth/actions/ReadAuthorizationResponse",
                            "https://kulzer-auth-fr.s2.belvgdev.com/mooauth/actions/ReadAuthorizationResponse",
                            "https://kulzer-auth-au.s2.belvgdev.com/mooauth/actions/ReadAuthorizationResponse"
                        };

                        var corsOriginsToAdd = new List<string>
                        {
                            "https://kulzer-auth-us.s2.belvgdev.com",
                            "https://kulzer-auth-dk.s2.belvgdev.com",
                            "https://kulzer-auth-fi.s2.belvgdev.com",
                            "https://kulzer-auth-fr.s2.belvgdev.com",
                            "https://kulzer-auth-au.s2.belvgdev.com",
                        };

                        var existingRedirectUris = client.RedirectUris.Select(u => u.RedirectUri).ToHashSet(StringComparer.OrdinalIgnoreCase);
                        var existingCorsOrigins =  client.AllowedCorsOrigins.Select(o => o.Origin).ToHashSet(StringComparer.OrdinalIgnoreCase);

                        foreach (var uri in redirectUrisToAdd)
                        {
                            if (!existingRedirectUris.Contains(uri))
                            {

                                client.RedirectUris.Add(new Duende.IdentityServer.EntityFramework.Entities.ClientRedirectUri
                                {
                                    RedirectUri = uri
                                });
                            }
                        }

                        foreach (var origin in corsOriginsToAdd)
                        {
                            if (!existingCorsOrigins.Contains(origin))
                            {
                                client.AllowedCorsOrigins.Add(new Duende.IdentityServer.EntityFramework.Entities.ClientCorsOrigin
                                {
                                    Origin = origin
                                });
                            }
                        }

                        configurationDbContext.SaveChanges();
                        Log.Information("Updated RedirectUris and ClientCorsOrigins for client m2shop-belvg.");
                    }
                    else
                    {
                        Log.Warning("Client with ClientId 'm2shop-belvg' was not found in the database.");
                    }


            Log.Information("Done updating database.");
        }
    }


}