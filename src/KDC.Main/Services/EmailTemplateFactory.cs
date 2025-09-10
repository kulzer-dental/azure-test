using KDC.Main.Constants;

namespace KDC.Main.Services
{
    public class EmailTemplateFactory: IEmailTemplateFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _templateTypes;

        public EmailTemplateFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _templateTypes = new Dictionary<string, Type>
            {
                [EmailConstants.ResetPassword] = typeof(ResetPasswordTemplate),
                [EmailConstants.ConfirmEmail] = typeof(ConfirmEmailTemplate),
                [EmailConstants.TwoFactor] = typeof(TwoFactorTemplate),
            };
        }

        public IEmailTemplate GetTemplate(string templateName)
        {
            if (!_templateTypes.TryGetValue(templateName, out var templateType))
                throw new ArgumentException($"No template found for name: {templateName}", nameof(templateName));

            return (IEmailTemplate)_serviceProvider.GetRequiredService(templateType);
        }
    }
}
