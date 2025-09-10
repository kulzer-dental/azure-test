using KDC.Main.Data.Models;

namespace KDC.Main.Services
{
    public interface IUserMigrationService
    {
        Task<bool> MigrateUserFromMagentoAsync(string email, string password, string returnUrl, int isEmailConfirmationRequired, bool shouldBlockUserRecoverPassword = false);
        Task<ApplicationUser?> GetOrCreateUserFromMagentoAsync(string email, string password, string returnUrl);
    }
}
