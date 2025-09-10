using System.Security.Claims;
using IdentityModel;
using KDC.Main.Config;
using KDC.Main.Data.Models;
using Microsoft.AspNetCore.Identity;
using Serilog;

internal static partial class HostingExtensions
{
    /// <summary>
    /// Ensure seed users exist
    /// </summary>
    /// <param name="app"></param>
    public static void EnsureSeedUsers(this WebApplication app)
    {
        Log.Information("Seeding users...");

        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var configurationManager = app.Services.GetRequiredService<IConfiguration>();
            var seedUsers = configurationManager.GetSection("SeedUsers").Get<SeedUser[]>();

            if (seedUsers != null)
            {
                foreach (var seedUser in seedUsers)
                {
                    if (String.IsNullOrEmpty(seedUser.UserName) || String.IsNullOrEmpty(seedUser.Password))
                    {
                        continue;
                    }

                    var user = userMgr.FindByNameAsync(seedUser.UserName).Result;
                    if (user == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = seedUser.UserName,
                            Email = seedUser.UserName,
                            EmailConfirmed = true,
                            StoreCode = "default", //Set to use default store code for seed
                        };

                        var result = userMgr.CreateAsync(user, seedUser.Password).Result;
                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }

                        

                        // Make sure user is not locked
                        result = userMgr.SetLockoutEnabledAsync(user, false).Result;

                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }

                        var claims = new List<Claim>();

                        if (!String.IsNullOrEmpty(seedUser.GivenName))
                        {
                            claims.Add(new Claim(JwtClaimTypes.GivenName, seedUser.GivenName));
                        }

                        if (!String.IsNullOrEmpty(seedUser.FamilyName))
                        {
                            claims.Add(new Claim(JwtClaimTypes.FamilyName, seedUser.FamilyName));
                        }

                        if (!String.IsNullOrEmpty(seedUser.GivenName) && !String.IsNullOrEmpty(seedUser.FamilyName))
                        {
                            claims.Add(new Claim(AppClaimTypes.DisplayName, $"{seedUser.GivenName} {seedUser.FamilyName}"));
                        }

                        if (!String.IsNullOrEmpty(seedUser.Culture))
                        {
                            claims.Add(new Claim(AppClaimTypes.Culture, seedUser.Culture));
                        }

                        result = userMgr.AddClaimsAsync(user, claims).Result;

                        if (!result.Succeeded)
                        {
                            throw new Exception(result.Errors.First().Description);
                        }

                        // Assign roles
                        if (seedUser.Roles != null) {
                           var roleAddResult = userMgr.AddToRolesAsync(user, seedUser.Roles).Result;
                           if (!roleAddResult.Succeeded)
                            {
                                throw new Exception(result.Errors.First().Description);
                            }
                        }

                        Log.Debug($"user {seedUser.UserName} created");
                    }
                    else
                    {
                        Log.Debug($"{seedUser.UserName} exists. skipping.");
                    }
                }
            }
        }

        Log.Information("Done seeding users. Exiting.");
    }
}