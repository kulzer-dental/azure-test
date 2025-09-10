using KDC.Main.Constants;
using KDC.Main.Services.Models;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace KDC.Main.Services
{
    public class ConfirmEmailTemplate: BaseEmailTemplate
    {
        public override string TemplateName => EmailConstants.ConfirmEmail;

        public ConfirmEmailTemplate() : base()
        {
        }

        public override async Task<string> RenderAsync(string templateContent, CultureInfo culture, object model)
        {
            if (model is not ConfirmEmailModel confirmModel)
                throw new ArgumentException("Model must be of type ConfirmEmailModel", nameof(model));

            var replacements = new Dictionary<string, string>
            {
                ["{helloMsg}"] = GetLocalizedString("Hello", culture),
                ["{confirmMsg}"] = GetLocalizedString("Please confirm your email address.", culture),
                ["{CONFIRMEMAIL}"] = GetLocalizedString("CONFIRM EMAIL", culture),
                ["{btnNotSeen}"] = GetLocalizedString("If the button above doesn't work, just copy and paste the following URL into the browser:", culture),
                ["{link}"] = confirmModel.ConfirmUrl
            };

            return await Task.FromResult(ReplaceMultiplePlaceholders(templateContent, replacements));
        }       
    }
}
