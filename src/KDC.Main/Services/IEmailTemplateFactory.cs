namespace KDC.Main.Services
{
    public interface IEmailTemplateFactory
    {
        IEmailTemplate GetTemplate(string templateName);
    }
}
