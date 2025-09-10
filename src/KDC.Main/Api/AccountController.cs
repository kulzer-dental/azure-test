using System.Globalization;
using IdentityModel;
using KDC.Main.Config;
using KDC.Main.Data.Models;
using KDC.Main.Models;
using Localization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace KDC.Main.Api
{
    /// <summary>
    /// Current user's API
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(
        UserManager<ApplicationUser> UserManager
    ) : ControllerBase
    {
        /// <summary>
        /// Returns your user profile
        /// </summary>
        /// <returns></returns>
        [HttpGet("me")]
        public async Task<UserProfileViewModel> GetMe()
        {
            var user = await UserManager.FindByNameAsync(User.Identity!.Name!)!;
            var claims = await UserManager.GetClaimsAsync(user!);

            return new UserProfileViewModel {
                 GivenName = claims.FirstOrDefault(c => c.Type == JwtClaimTypes.GivenName)?.Value,
                 FamilyName = claims.FirstOrDefault(c => c.Type == JwtClaimTypes.FamilyName)?.Value,
                 DisplayName = claims.FirstOrDefault(c => c.Type == AppClaimTypes.DisplayName)?.Value,
                 PreferredCulture = claims.FirstOrDefault(c => c.Type == AppClaimTypes.Culture)?.Value,
                 Email = user!.Email,
            };
        }
    }
}
