using System.Globalization;

namespace KDC.Main.Services
{
    public interface IEmailTemplate
    {
        string TemplateName { get; }
        Task<string> RenderAsync(string templateContent, CultureInfo culture, object model);
    }
}
