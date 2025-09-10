using System.Globalization;

namespace KDC.Main.Services;

public class EmailTemplateResult
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public interface ITemplateEngine
{
    EmailTemplateResult RenderTemplateAsync(string templateName, CultureInfo language, object model);
} 