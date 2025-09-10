using System.IO.Compression;
using Hangfire;
using KDC.Main.Config;
using KDC.Main.Security;
using Microsoft.Extensions.Options;

internal static partial class HostingExtensions
{

    /// <summary>
    /// Configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Security Headers Policy
        var securityHeadersPolicyCollection = new HeaderPolicyCollection()
            .AddFrameOptionsDeny() // Block embedding this page in iframes for older browsers
            .AddContentTypeOptionsNoSniff()
            .AddReferrerPolicyStrictOriginWhenCrossOrigin()
            .RemoveServerHeader()
            .AddContentSecurityPolicy(builder =>
            {
                builder.AddUpgradeInsecureRequests(); // upgrade-insecure-requests
                builder.AddBlockAllMixedContent(); // block-all-mixed-content

                // builder.AddReportUri() // report-uri: https://report-uri.com
                // .To("https://report-uri.com");

                builder.AddObjectSrc().Self();

                var trustedFormOrigins = app.Configuration.GetSection("Security:AllowedFormOrigins")
                    .Get<List<string>>() ?? new List<string>();

                var formBuilder = builder.AddFormAction().Self();
                foreach (var origin in trustedFormOrigins)
                {
                    formBuilder.From(origin);
                }

                builder.AddImgSrc()
                    .Data() // Allows placing images using data:, needed for css theme
                    .Self();

                builder.AddScriptSrc()
                    .Self()
                    // .UnsafeInline() // DO NOT USE UNLESS REALLY NEEDED!
                    // .UnsafeEval() // DO NOT USE UNLESS REALLY NEEDED!
                    ;

                // Block embedding this page in iframes
                builder.AddFrameAncestors().None();

                // Block external stylesheets
                builder.AddStyleSrc()
                    .Self();

                // Block external fonts
                builder.AddFontSrc()
                    .Self();
            });

        if (app.Environment.IsDevelopment())
        {
            // Convinience to run db migrations from web
            app.UseMigrationsEndPoint();
        }
        else
        {
            // Use custom error pages
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            // We use https://github.com/andrewlock/NetEscapades.AspNetCore.SecurityHeaders instead of native implementation
            securityHeadersPolicyCollection.AddStrictTransportSecurityMaxAgeIncludeSubDomains(maxAgeInSeconds: 60 * 60 * 24 * 365); // maxage = one year in seconds
        }

        // Redirect to HTTPs if accessed from HTTP
        app.UseHttpsRedirection();

        // Host static files from wwwroot directoy
        app.UseStaticFiles();

        // Enable routing features for web pages and api's
        app.UseRouting();

        // Enable Identity Server endpoints
        app.UseIdentityServer();

        // Enable authorization features
        app.UseAuthentication();
        app.UseAuthorization();

        // I18n
        var options = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
        app.UseRequestLocalization();

        // Restrict Access to swagger paths
        app.UseSwaggerUIAuthorization();

        // Add OpenAPI 3.0 document serving middleware
        // Available at: http://localhost:<port>/swagger/v1/swagger.json
        app.UseOpenApi();

        // Add web UIs to interact with the document
        // Available at: http://localhost:<port>/swagger
        // To generate API clients see: https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-nswag?view=aspnetcore-8.0&tabs=visual-studio
        app.UseSwaggerUi(); // UseSwaggerUI Protected by if (env.IsDevelopment())

        // Add authorization to hangfire dashboard
        var hangfireAuthFilter = new HangfireAuthorizationFilter();
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { hangfireAuthFilter },
            IsReadOnlyFunc = hangfireAuthFilter.IsReadOnly,
        });

        // Apply Security Headers Policy
        app.UseSecurityHeaders(securityHeadersPolicyCollection);

        // Map razor pages to routing
        app.MapRazorPages();
        //.RequireAuthorization(); // Require global authorization for all razor pages

        // Enable CORS Policy
        app.UseCors(CorsConfig.PolicyName);

        // Enable Routing to API Controllers
        app.MapControllers();
        //.RequireAuthorization(); // Require global authorization for all controllers

        return app;
    }

}