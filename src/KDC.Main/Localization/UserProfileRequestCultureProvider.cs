using KDC.Main.Config;
using KDC.Main.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;

public class UserProfileRequestCultureProvider : RequestCultureProvider
{
    public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        if (httpContext == null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        var userName = httpContext.User.Identity?.Name;
        if (userName != null) {
            var userManager = httpContext.RequestServices.GetService<UserManager<ApplicationUser>>()!;
            var user = await userManager.FindByNameAsync(userName!);
            var claims = await userManager.GetClaimsAsync(user!);
            var userCulture = claims.FirstOrDefault(c => c.Type == AppClaimTypes.Culture)?.Value;

            if (userCulture != null) {
                return new ProviderCultureResult(userCulture ?? null);
            }
            
        }

        return null;
    }
}