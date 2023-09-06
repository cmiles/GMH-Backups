using GmhWorkshop.Web;
using GmhWorkshop.Web.TaskWrappers;
using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Extensions;
using Hangfire.Console.Extensions.Serilog;
using Hangfire.Dashboard;
using Hangfire.Storage.SQLite;
using Serilog;
using Serilog.Enrichers.CallerInfo;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .Enrich.WithCallerInfo(true,
        "GmhWorkshop.",
        "gmhworkshop")
    .Enrich.WithHangfireContext()
    .MinimumLevel.Override("Hangfire", LogEventLevel.Verbose)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.Hangfire()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseConsole()
    .UseSQLiteStorage());

builder.Services.AddScoped<BirdPiBackupHangfireWrapper>();
builder.Services.AddScoped<TempestDayFileBackupHangfireWrapper>();
builder.Services.AddScoped<SensorPushDayFileBackupHangfireWrapper>();

// Add the processing server as IHostedService
builder.Services.AddHangfireServer();
builder.Services.AddHangfireConsoleExtensions();
// Add framework services.
builder.Services.AddMvc();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapHangfireDashboard(new DashboardOptions { Authorization = new[] { new HangfireAllowAllAuthorizationFilter() } });
app.MapDefaultControllerRoute();
app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");


RecurringJob.AddOrUpdate<BirdPiBackupHangfireWrapper>("BirdPiBackup", x => x.Run(null), "0 1 * * 0");
RecurringJob.AddOrUpdate<TempestDayFileBackupHangfireWrapper>("TempestDayFileBackup", x => x.Run(null), "0 2 * * *");
RecurringJob.AddOrUpdate<SensorPushDayFileBackupHangfireWrapper>("SensorPushDayFileBackup", x => x.Run(null),
    "0 2 * * *");

try
{
    app.Run();
}
catch (Exception e)
{
    Log.Fatal(e, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public class HangfireAllowAllAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Allow all users to see the Dashboard - Dangerous and assumes any/all access
        // control is from external network setup!?!
        return true;
    }
}