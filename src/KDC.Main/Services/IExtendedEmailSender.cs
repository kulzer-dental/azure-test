using Microsoft.AspNetCore.Identity.UI.Services;

namespace KDC.Main.Services
{
    public interface IExtendedEmailSender: IEmailSender
    {
        Task SendResetPasswordEmailAsync(string email, string resetUrl, string storeCode, string? userCulture = null);
        Task SendConfirmationEmailAsync(string email, string confirmUrl, string storeCode, string? userCulture = null);
        Task SendTwoFactorEmailAsync(string email, string twoFactorCode, string storeCode, string? userCulture = null);
    }
}
