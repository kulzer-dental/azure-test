using System.Globalization;
using IdentityModel;
using KDC.Main.Config;
using KDC.Main.Data.Models;
using KDC.Main.Models;
using Localization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace KDC.Main.Services;

public interface IEmailLocalizationService
{
    Task<CultureInfo> GetUserPreferredCulture(string userId);
    CultureInfo GetFallbackCulture();
}

public class EmailLocalizationService : IEmailLocalizationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly I18nConfig _i18nConfig;
    private readonly IConfiguration _configuration;

    public EmailLocalizationService(
        UserManager<ApplicationUser>  userManager,
        IConfiguration configuration,
        I18nConfig i18nConfig
    )
    {
        _userManager = userManager;
        _configuration = configuration;
        _i18nConfig = i18nConfig;
    }

    public async Task<CultureInfo> GetUserPreferredCulture(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId)!;
            var claims = await _userManager.GetClaimsAsync(user!);
            var preferredCulture = claims.FirstOrDefault(c => c.Type == AppClaimTypes.Culture)?.Value;
            if (preferredCulture != null)
            {
                return new CultureInfo(preferredCulture);
            }
            
            return GetFallbackCulture();
        }
        catch
        {
            return GetFallbackCulture();
        }
    }

    public CultureInfo GetFallbackCulture()
    {
        var currentCulture = Thread.CurrentThread.CurrentUICulture;
        
        if (!IsSupportedCulture(currentCulture))
        {
            return new CultureInfo("en");
        }

        return currentCulture;
    }

    private bool IsSupportedCulture(CultureInfo culture)
    {
          var currentCulture = Thread.CurrentThread.CurrentUICulture;
            
            var result = new List<CultureViewModel>();
            var cultures =  _i18nConfig.SupportedCultures.Select(c => {
                var cultureInfo = new CultureInfo(c);
                return new CultureViewModel {
                    IsSystemDefault = _i18nConfig.DefaultCulture == cultureInfo.Name,
                    EnglishName = cultureInfo.EnglishName,
                    NativeName = cultureInfo.NativeName,
                    IsoCode = cultureInfo.Name
                };
            }).ToList();
            
        return cultures.Where(c => c.IsoCode == culture.Name).FirstOrDefault() != null;
    }
} 