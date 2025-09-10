using KDC.Main.Constants;
using KDC.Main.Services.Models;
using Localization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Resources;

namespace KDC.Main.Services;

public class EmailSender : IExtendedEmailSender
{
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templateService;
    private readonly IEmailTemplateFactory _templateFactory;
    private readonly IEmailLocalizationService _localizationService;
    private readonly ILogger<EmailSender> _logger;
    private readonly ResourceManager _resourceManager;

    public EmailSender(IEmailService emailService, IEmailTemplateService templateService,
         IEmailLocalizationService localizationService, IEmailTemplateFactory templateFactory,
         ILogger<EmailSender> logger)
    {
        _emailService = emailService;
        _templateService = templateService;
        _localizationService = localizationService;
        _templateFactory = templateFactory;

        _resourceManager = new ResourceManager(
            "KDC.Main.Resources.Localization.EmailTemplate",
            typeof(Program).Assembly);
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return _emailService.SendEmailAsync(email, subject, htmlMessage);
    }

    public async Task SendResetPasswordEmailAsync(string email, string resetUrl, string storeCode, string? userCulture = null)
    {
        try
        {
            var culture = GetCultureFromString(userCulture);
            var template = _templateFactory.GetTemplate(EmailConstants.ResetPassword);
            var templateContent = await _templateService.GetEmailBodyAsync(htmlTemplate: EmailConstants.ResetPassword, storeCode, userCulture);

            var model = new ResetPasswordModel
            {
                ResetUrl = resetUrl,
                UserEmail = email
            };

            var emailBody = await template.RenderAsync(templateContent, culture, model);

            if (string.IsNullOrEmpty(emailBody))
                throw new Exception("Email body cannot be null or empty");

            emailBody = emailBody.Replace("{link}", resetUrl);
            var emailSubject = GetLocalizedString("Kultzer - Reset password", culture);

            await _emailService.SendEmailAsync(email, emailSubject, emailBody);
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(ex, "Failed to send reset password email");
        }
    }

    public async Task SendConfirmationEmailAsync(string email, string confirmUrl, string storeCode, string? userCulture = null)
    {
        try
        {
            var culture = GetCultureFromString(userCulture);
            var template = _templateFactory.GetTemplate(EmailConstants.ConfirmEmail);
            var templateContent = await _templateService.GetEmailBodyAsync(htmlTemplate: EmailConstants.ConfirmEmail, storeCode, userCulture);

            var model = new ConfirmEmailModel
            {
                ConfirmUrl = confirmUrl,
                UserEmail = email
            };

            var emailBody = await template.RenderAsync(templateContent, culture, model);
            var emailSubject = GetLocalizedString("Kultzer - Confirm your email adress", culture);

            if (string.IsNullOrEmpty(emailBody))
                throw new Exception("Email body cannot be null or empty");

            emailBody = emailBody.Replace("{link}", confirmUrl);

            await _emailService.SendEmailAsync(email, emailSubject, emailBody);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error while sending the confirmation email");
        }
    }

    public async Task SendTwoFactorEmailAsync(string email, string twoFactorCode, string storeCode, string? userCulture = null)
    {
        try
        {
            var culture = GetCultureFromString(userCulture);
            var template = _templateFactory.GetTemplate(EmailConstants.TwoFactor);
            var templateContent = await _templateService.GetEmailBodyAsync(htmlTemplate: EmailConstants.TwoFactor, storeCode, userCulture);

            var model = new TwoFactorEmailModel
            {
                TwoFactorCode = twoFactorCode,
                UserEmail = email
            };

            var emailBody = await template.RenderAsync(templateContent, culture, model);
            var emailSubject = GetLocalizedString("Kultzer - Two Factor Code", culture);

            if (string.IsNullOrEmpty(emailBody))
                throw new Exception("Email body cannot be null or empty");

            await _emailService.SendEmailAsync(email, emailSubject, emailBody);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error while sending the two factor email");
        }

    }

    private CultureInfo GetCultureFromString(string? userCulture)
    {
        if (string.IsNullOrEmpty(userCulture))
        {
            return _localizationService.GetFallbackCulture();
        }

        try
        {
            return new CultureInfo(userCulture);
        }
        catch (CultureNotFoundException)
        {
            return _localizationService.GetFallbackCulture();
        }
    }

    protected string GetLocalizedString(string key, CultureInfo? culture)
    {
        return _resourceManager.GetString(key, culture) ?? key;
    }
}
