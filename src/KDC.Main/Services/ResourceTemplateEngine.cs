using Microsoft.Extensions.Localization;
using System.Globalization;
using FluentEmail.Core;
using Localization;

namespace KDC.Main.Services;

public class ResourceTemplateEngine : ITemplateEngine 
{
    private readonly IStringLocalizer<Localization.EmailTemplate> _localizer;
    private readonly IFluentEmailFactory _fluentEmailFactory;

    public ResourceTemplateEngine(
        IStringLocalizer<Localization.EmailTemplate> localizerFactory,
        IFluentEmailFactory fluentEmailFactory)
    {
        var type = typeof(Localization.EmailTemplate);
        _localizer = localizerFactory;
        _fluentEmailFactory = fluentEmailFactory;
    }

    public EmailTemplateResult RenderTemplateAsync(string templateName, CultureInfo language, object model)
    {
        using (new CultureScope(language))
        {
            CultureInfo.CurrentCulture = language;
            CultureInfo.CurrentUICulture = language;
            var subjectKey = $"{templateName}_Subject";
            var bodyKey = $"{templateName}_Body";
            var subject = _localizer[subjectKey].Value;
            var template = _localizer[bodyKey].Value;
            var body = _fluentEmailFactory.Create()
                .UsingTemplate(template, model).Data;

            return new EmailTemplateResult
            {
                Subject = subject,
                Body = body.Body?.ToString() ?? string.Empty
            };
        }
    }
}