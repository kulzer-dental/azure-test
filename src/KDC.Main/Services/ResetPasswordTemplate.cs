using KDC.Main.Constants;
using KDC.Main.Services.Models;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace KDC.Main.Services
{
    public class ResetPasswordTemplate: BaseEmailTemplate
    {
        public override string TemplateName => EmailConstants.ResetPassword;

        public ResetPasswordTemplate() : base()
        {
        }

        public override async Task<string> RenderAsync(string templateContent, CultureInfo culture, object model)
        {
            if (model is not ResetPasswordModel resetModel)
                throw new ArgumentException("Model must be of type ResetPasswordModel", nameof(model));

            var replacements = new Dictionary<string, string>
            {
                ["{helloMsg}"] = GetLocalizedString("Hello", culture),
                ["{resetPasswordMessage}"] = GetLocalizedString("We received a request to reset your password. Click the button below to reset it", culture),
                ["{RESETPASSWORD}"] = GetLocalizedString("RESET PASSWORD", culture),
                ["{btnNotSeen}"] = GetLocalizedString("If the button above doesn't work, just copy and paste the following URL into the browser:", culture),
                ["{link}"] = resetModel.ResetUrl
            };

            return await Task.FromResult(ReplaceMultiplePlaceholders(templateContent, replacements));
        }
       
    }
}
