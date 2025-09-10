using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Resources;

namespace KDC.Main.Services
{
    public abstract class BaseEmailTemplate: IEmailTemplate
    {
        private readonly ResourceManager _resourceManager;

        protected BaseEmailTemplate()
        {
            _resourceManager = new ResourceManager(
            "KDC.Main.Resources.Localization.EmailTemplate",
            typeof(Program).Assembly);
        }

        public abstract string TemplateName { get; }

        public abstract Task<string> RenderAsync(string templateContent, CultureInfo culture, object model);

        protected string ReplaceMultiplePlaceholders(string template, Dictionary<string, string> replacements)
        {
            foreach (var replacement in replacements)
            {
                template = template.Replace(replacement.Key, replacement.Value);
            }
            return template;
        }

        protected string GetLocalizedString(string key, CultureInfo? culture)
        {
            return _resourceManager.GetString(key, culture) ?? key;
        }
    }
}
