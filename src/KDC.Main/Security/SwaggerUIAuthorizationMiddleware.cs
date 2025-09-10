using System.Net;
using System.Text;
using KDC.Main.Security;

namespace KDC.Main.Security
{
    public class SwaggerUIAuthorizationMiddleware
    {
        private readonly RequestDelegate next;

        public SwaggerUIAuthorizationMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                var hasAccess = context.User.IsInRole(AppRoles.Developer);
                if (hasAccess)
                {
                    await next.Invoke(context);
                    return;
                }

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
            else
            {
                await next.Invoke(context);
            }
        }
    }

    public static class SwaggerUIAuthorizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseSwaggerUIAuthorization(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SwaggerUIAuthorizationMiddleware>();
        }
    }
}