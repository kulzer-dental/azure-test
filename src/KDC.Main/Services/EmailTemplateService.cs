using KDC.Main.Constants;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Resources;

namespace KDC.Main.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<EmailTemplateService> _logger;
        private readonly IStoreContactService _storeContact;
        private readonly ResourceManager _resourceManager;
        private string? _cachedLayout;

        public EmailTemplateService(IWebHostEnvironment environment, ILogger<EmailTemplateService> logger,
                IStoreContactService storeContact)
        {
            _environment = environment;
            _logger = logger;
            _storeContact = storeContact;

            _resourceManager = new ResourceManager(
            "KDC.Main.Resources.Localization.EmailTemplate",
            typeof(Program).Assembly);
        }

        public async Task<string> GetDefaultEmailLayoutAsync()
        {
            if (_cachedLayout != null)
                return _cachedLayout;

            var layoutPath = Path.Combine(_environment.ContentRootPath, "Templates", EmailConstants.EmailTemplateDirectory);

            if (!File.Exists(layoutPath))
            {
                _logger.LogError("Email layout template not found at: {LayoutPath}", layoutPath);
                throw new FileNotFoundException($"Email layout template not found at: {layoutPath}");
            }

            _cachedLayout = await File.ReadAllTextAsync(layoutPath);
            return _cachedLayout;
        }

        public async Task<string> GetEmailBodyAsync(string htmlTemplate, string storeCode, string? userCulture = null)
        {
            if (string.IsNullOrEmpty(htmlTemplate))
                throw new ArgumentException("Template name cannot be null or empty", nameof(htmlTemplate));

            try
            {
                var layout = await GetDefaultEmailLayoutAsync();
                var templateContent = await GetTemplateContentAsync(htmlTemplate);

                var storeContact = _storeContact.GetStoreContacts(storeCode);

                var culture = string.IsNullOrEmpty(userCulture) ? CultureInfo.InvariantCulture : new CultureInfo(userCulture);

                var contactMessage1 = GetLocalizedString("Should you need to get in touch with us or require technical assistance, please email us at", culture);
                var contactMessage2 = GetLocalizedString("and we'll get back to you in 24h at most.", culture);
                var contactUs = GetLocalizedString("Contact us", culture);
                var privacyPolicy = GetLocalizedString("Privacy Policy", culture);

                var body = layout.Replace("{htmlContent}", templateContent)
                            .Replace("{emailAddress}", storeContact?.SupportEmail)
                            .Replace("{contactMessage1}", contactMessage1)
                            .Replace("{contactMessage2}", contactMessage2)
                            .Replace("{contactUrl}", storeContact?.ContactPageUrl)
                            .Replace("{contactUS}", contactUs)
                            .Replace("{privacyPolicyUrl}", storeContact?.PrivacyPolicyUrl)
                            .Replace("{privacyPolicy}", privacyPolicy);

                return body;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating email body for template: {Template}", htmlTemplate);
                throw;
            }
        }

        private async Task<string> GetTemplateContentAsync(string templateName)
        {
            var templatePath = Path.Combine(_environment.ContentRootPath, "Templates", templateName);

            if (!File.Exists(templatePath))
            {
                _logger.LogError("Email template not found at: {TemplatePath}", templatePath);
                throw new FileNotFoundException($"Email template not found at: {templatePath}");
            }

            return await File.ReadAllTextAsync(templatePath);
        }

        public string GetLocalizedString(string key, CultureInfo? culture)
        {
            return _resourceManager.GetString(key, culture) ?? key;
        }
    }
}
