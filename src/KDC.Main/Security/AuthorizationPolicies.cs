namespace KDC.Main.Security
{
    public static class AuthorizationPolicies
    {
        public static string AdminOnly = nameof(AdminOnly);
        public static string AccessHangfireDashboard = nameof(AccessHangfireDashboard);
        public static string TwoFactorEnabled = nameof(TwoFactorEnabled);
    }
}