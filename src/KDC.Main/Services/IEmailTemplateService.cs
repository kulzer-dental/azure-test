namespace KDC.Main.Services
{
    public interface IEmailTemplateService
    {
        Task<string> GetDefaultEmailLayoutAsync();
        Task<string> GetEmailBodyAsync(string htmlTemplate, string storeCode, string? userCulture = null);

    }
}
