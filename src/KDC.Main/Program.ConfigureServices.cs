using FluentEmail.Core;
using Hangfire;
using KDC.Main.Config;
using KDC.Main.Data;
using KDC.Main.Data.Models;
using KDC.Main.Repositories;
using KDC.Main.Security;
using KDC.Main.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Options;
using NSwag;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Globalization;
using System.Net;
using System.Net.Mail;

internal static partial class HostingExtensions
{

    /// <summary>
    /// Configures services for dependency injection
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    /// <exception cref="ConnectionStringNotFoundException"></exception>
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        // Get our default connection string from configuration
        var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                                    ?? throw new ConnectionStringNotFoundException("Connection string 'DefaultConnection' not found.");
        // Serilog logging framework
        builder.Host.UseSerilog((ctx, lc) => lc
                    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                    .WriteTo.MSSqlServer(connectionString: defaultConnectionString, sinkOptions: new MSSqlServerSinkOptions
                    {
                        SchemaName = "Serilog",
                        TableName = "LogEvents",
                        AutoCreateSqlDatabase = true,
                        AutoCreateSqlTable = true
                    })
                    .Enrich.FromLogContext()
                    .ReadFrom.Configuration(ctx.Configuration));

        // Setup main application db context
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                        defaultConnectionString,
                        o => o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, ApplicationDbContext.SchemaName)
                        );
        });

        builder.Services.AddHttpClient<IMagentoService, MagentoService>((client) =>
        {
            var timeoutSettings = builder.Configuration.GetValue<double>("HttpSettings:TimeoutSeconds");
            client.Timeout = TimeSpan.FromSeconds(timeoutSettings);
        });

        // Show error pages for database errors
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // Add Hangfire services.
        builder.Services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSerilogLogProvider()
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(defaultConnectionString))
            .AddHangfireServer();

        // Adds support for sending emails
        builder.Services.AddTransient<IEmailService, EmailService>();

        // Adds mail sender for ASP.Net Identity
        builder.Services.AddTransient<IEmailSender, EmailSender>();
        // Adds sms sender for ASP.Net Identity
        // builder.Services.AddTransient<ISmsSender, SmsSender>();

        // Adds services and configuration for asp.net identity
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                        {
                            // options.SignIn.RequireConfirmedAccount = false;
                            // options.SignIn.RequireConfirmedEmail = false;
                            // options.SignIn.RequireConfirmedPhoneNumber = false;
                            // options.SignIn.RequireConfirmedEmail = true;
                            options.SignIn.RequireConfirmedAccount = true;
                            options.Password.RequireDigit = true;
                            options.Password.RequireLowercase = true;
                            options.Password.RequireUppercase = true;
                            options.Password.RequireNonAlphanumeric = false;
                            options.Password.RequiredLength = 8;
                            options.Password.RequiredUniqueChars = 1;

                        })
                        .AddEntityFrameworkStores<ApplicationDbContext>()
                        .AddDefaultTokenProviders();

        // Add authentication options
        // See: https://github.com/aspnet-contrib/AspNet.Security.OAuth.Providers
        var authProviders = builder.Services.AddAuthentication();

        // Get external Auth Config
        //var externalAuthConfigs = builder.Configuration.GetSection("ExternalAuth").Get<ExternalAuthConfig[]>()
        //                ?? throw new NullReferenceException("ExternalAuth config must exist");


        builder.Services.Configure<ApiEndpoints>(builder.Configuration.GetSection("Magento:ApiEndpoints"));
        builder.Services.Configure<MagentoApiAuthentication>(builder.Configuration.GetSection("Magento:Authentication"));

        // Add Microsoft Account Login
        //var externalAuthMicrosoft = externalAuthConfigs.FirstOrDefault(a => a.Provider == "MicrosoftAccount");
        //if (externalAuthMicrosoft != null && externalAuthMicrosoft.Enabled)
        //{
        //    authProviders.AddMicrosoftAccount(
        //        options =>
        //            {
        //                options.ClientId = externalAuthMicrosoft.ClientId;
        //                options.ClientSecret = externalAuthMicrosoft.ClientSecret;
        //            }
        //    );
        //}

        //// Add Google Account Login
        //var externalAutGoogle = externalAuthConfigs.FirstOrDefault(a => a.Provider == "Google");
        //if (externalAutGoogle != null && externalAutGoogle.Enabled)
        //{
        //    authProviders.AddGoogle(
        //        options =>
        //            {
        //                options.ClientId = externalAutGoogle.ClientId;
        //                options.ClientSecret = externalAutGoogle.ClientSecret;
        //            }
        //    );
        //}

        //// Add Apple Account Login
        //var externalAutApple = externalAuthConfigs.FirstOrDefault(a => a.Provider == "Apple");
        //if (externalAutApple != null && externalAutApple.Enabled)
        //{
        //    authProviders.AddApple(
        //        options =>
        //            {
        //                options.ClientId = externalAutApple.ClientId;
        //                options.ClientSecret = externalAutApple.ClientSecret;
        //            }
        //    );
        //}

        // Configure cookie authentication options
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.AccessDeniedPath = new PathString("/Identity/Account/AccessDenied");
            // options.Cookie.Name = "Cookie";
            // options.Cookie.HttpOnly = true;
            // options.ExpireTimeSpan = TimeSpan.FromMinutes(720);
            options.LoginPath = new PathString("/Identity/Account/Login");
            // options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
            // options.SlidingExpiration = true;
        });

        // Authorization policies
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireRole(AppRoles.Admin));

            options.AddPolicy(AuthorizationPolicies.TwoFactorEnabled, x => x.RequireClaim("amr", "mfa"));

            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
        });

        // Provice localizer service for i18n
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

        // Service to apply cultures in middleware
        var i18nSection = builder.Configuration.GetSection("I18n");

        if (!i18nSection.Exists())
        {
            throw new InvalidOperationException("I18n configuration section not found in appsettings.json");
        }

        var i18nConfig = new I18nConfig
        {
            DefaultCulture = i18nSection["DefaultCulture"] ?? "en",
            SupportedCultures = i18nSection.GetSection("SupportedCultures").Get<string[]>()
                       ?? throw new InvalidOperationException("SupportedCultures not found in I18n configuration")
        };

        builder.Services.AddSingleton<I18nConfig>(i18nConfig);

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {

            var supportedCultures = new List<CultureInfo>();
            foreach (var c in i18nConfig.SupportedCultures)
            {
                supportedCultures.Add(new CultureInfo(c));
            }

            options.DefaultRequestCulture = new RequestCulture(i18nConfig.DefaultCulture);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.FallBackToParentCultures = true;
            options.FallBackToParentUICultures = true;

            options.RequestCultureProviders = new List<IRequestCultureProvider>()
            {
                new UserProfileRequestCultureProvider(),
                new CookieRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider(),
                new QueryStringRequestCultureProvider(),
            };
        });

        builder.Services.Configure<StoresContactsConfig>(builder.Configuration.GetSection("StoresContacts"));

        // Adds services to render razor pages
        builder.Services.AddRazorPages(options =>
        {
            // Authorization policies
            // See: https://learn.microsoft.com/en-us/aspnet/core/security/authorization/razor-pages-authorization?view=aspnetcore-8.0
            options.Conventions
                                // Error Pages
                                .AllowAnonymousToPage("/Error")
                                .AllowAnonymousToPage("/NotFound")
                                // Helper Pages
                                .AllowAnonymousToPage("/SetCulture")
                                // Identity Area
                                .AuthorizeAreaFolder("Identity", "/Account")
                                .AllowAnonymousToAreaPage("Identity", "/Account/AccessDenied")
                                .AllowAnonymousToAreaPage("Identity", "/Account/ConfirmEmail")
                                .AllowAnonymousToAreaPage("Identity", "/Account/ConfirmEmailChange")
                                .AllowAnonymousToAreaPage("Identity", "/Account/ExternalLogin")
                                .AllowAnonymousToAreaPage("Identity", "/Account/ForgotPassword")
                                .AllowAnonymousToAreaPage("Identity", "/Account/ForgotPasswordConfirmation")
                                .AllowAnonymousToAreaPage("Identity", "/Account/Lockout")
                                .AllowAnonymousToAreaPage("Identity", "/Account/Logout")
                                .AllowAnonymousToAreaPage("Identity", "/Account/Login")
                                .AllowAnonymousToAreaPage("Identity", "/Account/LoginWith2fa")
                                .AllowAnonymousToAreaPage("Identity", "/Account/LoginWith2faEmail")
                                .AllowAnonymousToAreaPage("Identity", "/Account/LoginWith2faAuthenticator")
                                .AllowAnonymousToAreaPage("Identity", "/Account/LoginWithRecoveryCode")
                                .AllowAnonymousToAreaPage("Identity", "/Account/Register")
                                .AllowAnonymousToAreaPage("Identity", "/Account/RegisterConfirmation")
                                .AllowAnonymousToAreaPage("Identity", "/Account/ResetPassword")
                                .AllowAnonymousToAreaPage("Identity", "/Account/ResetPasswordConfirmation")

                                // Admin Area
                                // Pages that are not in a subfolder must be specified explitily
                                .AuthorizeAreaPage("Admin", "/Index", AuthorizationPolicies.AdminOnly)
                                // Pages in a subfolder can be specified by folder
                                // .AuthorizeAreaFolder("Admin", "/Subfolder", AuthorizationPolicies.AdminOnly)
                                ;
        })
        // Add i18n support
        .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
        .AddDataAnnotationsLocalization();

        // Get CORS Config
        var corsSettings = builder.Configuration.GetSection("CORS").Get<CorsConfig>()
                        ?? throw new NullReferenceException("CORS config must exist");

        // Register CORS policy
         builder.Services.AddCors(options =>
        {
            options.AddPolicy(CorsConfig.PolicyName,
                                policy =>
                                {
                                    policy.WithOrigins(corsSettings.AllowedOrigins)
                                                        .AllowAnyHeader()
                                                        .AllowAnyMethod();
                                });
        });

        // Adds services for API controllers
        builder.Services.AddControllers();

        // Adds services for nswagv open ai
        builder.Services.AddOpenApiDocument(options =>
        {
            options.PostProcess = document =>
            {
                document.Info = new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Kulzer Digital Core API",
                    Description = "An for the Kulzer Digital Core",
                    Contact = new OpenApiContact
                    {
                        Name = "Kulzer GmbH",
                        Url = "https://www.kulzer.com"
                    },
                };
            };
        });


        // Add services for identity server
        builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                options.EmitStaticAudienceClaim = true;
            })
            .AddConfigurationStore(options =>
            {
                options.DefaultSchema = "Configuration";
                options.ConfigureDbContext = b =>
                {
                    b.UseSqlServer(
                        defaultConnectionString,
                        sql => sql
                            .MigrationsAssembly(typeof(Program).Assembly.GetName().Name)
                            .MigrationsHistoryTable(HistoryRepository.DefaultTableName, options.DefaultSchema)
                    );
                };
            })
            .AddOperationalStore(options =>
            {
                options.DefaultSchema = "PersistedGrant";
                options.ConfigureDbContext = b => b.UseSqlServer(
                    defaultConnectionString,
                    sql => sql
                        .MigrationsAssembly(typeof(Program).Assembly.GetName().Name)
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, options.DefaultSchema)
                    );
            })
            .AddAspNetIdentity<ApplicationUser>();

        // Fluent Mail services
        var smtpConfig = builder.Configuration.GetSection("Smtp").Get<SmtpConfig>() ?? throw new NullReferenceException("SMTP Config must not be null");

        var fluentMail = builder.Services
             .AddFluentEmail(smtpConfig.SenderEmail, smtpConfig.SenderName)
             .AddHandlebarsRenderer()
             .AddSmtpSender(new SmtpClient(smtpConfig.Host, smtpConfig.Port)
             {
                 Credentials = new NetworkCredential(smtpConfig.UserName, smtpConfig.Password),
                 EnableSsl = smtpConfig.UseSSL
             });

        // Add templates engine as Singleton
        builder.Services.AddSingleton<IFluentEmailFactory, FluentEmailFactory>();
        // FIXED: Changed from Singleton to Scoped because EmailTemplateService depends on IStoreContactService (Scoped)
        builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();

        // Add scoped services
        builder.Services.AddScoped<IStoreConfigurationService, StoreConfigurationService>();
        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IEmailSender, EmailSender>();
        builder.Services.AddScoped<IEmailLocalizationService, EmailLocalizationService>();
        builder.Services.AddScoped<IExtendedEmailSender, EmailSender>();
        builder.Services.AddScoped<ITemplateEngine, ResourceTemplateEngine>();
        builder.Services.AddScoped<IUserMigrationService, UserMigrationService>();
        builder.Services.AddScoped<IClientConfigurationRepository, ClientConfigurationRepository>();

        builder.Services.AddScoped<ResetPasswordTemplate>();
        builder.Services.AddScoped<ConfirmEmailTemplate>();
        builder.Services.AddScoped<TwoFactorTemplate>();

        builder.Services.AddScoped<IEmailTemplateFactory, EmailTemplateFactory>();

        builder.Services.AddScoped<IStoreContactService, StoreContactService>();

        // Builds all services
        return builder.Build();
    }

}