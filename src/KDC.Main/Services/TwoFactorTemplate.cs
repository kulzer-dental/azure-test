using KDC.Main.Constants;
using KDC.Main.Services.Models;
using System.Globalization;
using System.Text;

namespace KDC.Main.Services
{
    public class TwoFactorTemplate: BaseEmailTemplate
    {
        public override string TemplateName => EmailConstants.ConfirmEmail;

        public TwoFactorTemplate() : base()
        {
        }

        public override async Task<string> RenderAsync(string templateContent, CultureInfo culture, object model)
        {
            if (model is not TwoFactorEmailModel twoFactorModel)
                throw new ArgumentException("Model must be of type TwoFactorModel", nameof(model));

            StringBuilder twoFactorMessage = new StringBuilder(GetLocalizedString("Please enter your two factor code at the sign in page {twoFactorCode}", culture));
            twoFactorMessage = twoFactorMessage.Replace("{twoFactorCode}", twoFactorModel?.TwoFactorCode);


            var replacements = new Dictionary<string, string>
            {
                ["{helloMsg}"] = GetLocalizedString("Hello", culture),
                ["{twoFactorMessage}"] = twoFactorMessage.ToString(),
            };

            return await Task.FromResult(ReplaceMultiplePlaceholders(templateContent, replacements));
        }
    }
}
