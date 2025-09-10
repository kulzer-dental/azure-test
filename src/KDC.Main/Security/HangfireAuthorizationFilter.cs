using Hangfire.Dashboard;
using KDC.Main.Config;

namespace KDC.Main.Security
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        /// <summary>
        /// Grants access to hangfire dashboard
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool Authorize(DashboardContext context)
        {
            return context.GetHttpContext().User.IsInRole(AppRoles.Admin);
        }

        public bool IsReadOnly(DashboardContext context)
        {
            return context.GetHttpContext().User.IsInRole(AppRoles.Developer) == false;
        }
    }
}