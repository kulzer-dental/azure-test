using KDC.Main.Data.Models;

namespace KDC.Main.Helpers
{
    public static class UserFactory
    {
        public static ApplicationUser CreateUser(string storeCode)
        {
            if (string.IsNullOrWhiteSpace(storeCode))
            {
                throw new ArgumentException("StoreCode is required and cannot be null or empty.", nameof(storeCode));
            }

            return new ApplicationUser
            {
                StoreCode = storeCode
            };
        }
    }
}
