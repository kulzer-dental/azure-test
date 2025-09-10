using System.Security.Claims;
using IdentityModel;
using KDC.Main.Config;
using KDC.Main.Data.Models;
using KDC.Main.Security;
using Microsoft.AspNetCore.Identity;
using Serilog;

internal static partial class HostingExtensions
{
    /// <summary>
    /// Seeds application roles to database
    /// </summary>
    /// <param name="app"></param>
    /// <exception cref="Exception"></exception>
    public static void EnsureSeedRoles(this WebApplication app)
    {
        Log.Information("Seeding roles...");

        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            CreateRole(roleManager, AppRoles.Admin);
            CreateRole(roleManager, AppRoles.Developer);

            Log.Information("Seeding roles completed.");
        }
    }

    private static void CreateRole(RoleManager<IdentityRole> roleManager, string role)
    {
        var roleExists = roleManager.RoleExistsAsync(role).Result;
        if (!roleExists)
        {
            var result = roleManager.CreateAsync(new IdentityRole(role)).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            
            Log.Information($"Created Role {role}");
        }
    }
}