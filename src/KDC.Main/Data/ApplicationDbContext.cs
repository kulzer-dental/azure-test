using Duende.IdentityServer.EntityFramework.Entities;
using Duende.IdentityServer.EntityFramework.Stores;
using KDC.Main.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KDC.Main.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>
    /// Custom schema name as we use more than one schema
    /// </summary>
    public const string SchemaName = "Application";

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        // set custom schema name
        builder.HasDefaultSchema(SchemaName);

        // Customize table names to remove AspNet prefixes
        builder.Entity<ApplicationUser>(e => e.ToTable(name: "Users"));
        builder.Entity<IdentityRole>(e => e.ToTable(name: "Roles"));
        builder.Entity<IdentityUserRole<string>>(e => e.ToTable(name: "UserRoles"));
        builder.Entity<IdentityUserClaim<string>>(e => e.ToTable(name: "UserClaims"));
        builder.Entity<IdentityUserLogin<string>>(e => e.ToTable(name: "UserLogins"));
        builder.Entity<IdentityRoleClaim<string>>(e => e.ToTable(name: "RoleClaims"));
        builder.Entity<IdentityUserToken<string>>(e => e.ToTable(name: "UserTokens"));
    }
}
