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
    /// A demo controller for the api
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CultureController(
        I18nConfig i18nConfig
    ) : ControllerBase
    {
        /// <summary>
        /// Returns the current application culture
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        public CultureViewModel Get()
        {
            var currentCulture = Thread.CurrentThread.CurrentUICulture;

            return new CultureViewModel {
                IsSystemDefault = i18nConfig.DefaultCulture == currentCulture.Name,
                EnglishName = currentCulture.EnglishName,
                NativeName = currentCulture.NativeName,
                IsoCode = currentCulture.Name
            };
        }

        /// <summary>
        /// Returns a list of available application cultures
        /// </summary>
        /// <returns></returns>
        [HttpGet("supported")]
        public IEnumerable<CultureViewModel> GetSupported()
        {
            var currentCulture = Thread.CurrentThread.CurrentUICulture;
            
            var result = new List<CultureViewModel>();
            return i18nConfig.SupportedCultures.Select(c => {
                var cultureInfo = new CultureInfo(c);
                return new CultureViewModel {
                    IsSystemDefault = i18nConfig.DefaultCulture == cultureInfo.Name,
                    EnglishName = cultureInfo.EnglishName,
                    NativeName = cultureInfo.NativeName,
                    IsoCode = cultureInfo.Name
                };
            });
        }
    }
}
