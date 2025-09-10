using DotNetEnv.Configuration;
using Serilog;
using System.Diagnostics;

// Wire up Serilog for application lifecycle hooks
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

// Log startup
var environment = DotNetEnv.Env.GetString("ASPNETCORE_ENVIRONMENT");
Log.Information($"Starting up at \"{environment}\"");

try
{
    // Init web app builder
    var builder = WebApplication
        .CreateBuilder(args);

    // Build configuration (aka app settings)
    builder.Configuration
        .AddDotNetEnv("../../.env/main.env") // Set environment variables from env file
        .AddEnvironmentVariables(); // Overwrite using real environment variables 

    // Bootstrap application
    builder
        .ConfigureServices()
        .ConfigurePipeline()
        .ExecuteArgs(args)
        .Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    // See https://github.com/dotnet/runtime/issues/60600 re StopTheHostException
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
