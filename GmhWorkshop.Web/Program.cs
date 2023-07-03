using GmhWorkshop.Jobs;
using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Extensions;
using Hangfire.Console.Extensions.Serilog;
using Hangfire.Server;
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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseConsole()
    .UseInMemoryStorage());

builder.Services.AddScoped<BirdPiBackupHangFireWrapper>();

// Add the processing server as IHostedService
builder.Services.AddHangfireServer();
builder.Services.AddHangfireConsoleExtensions();
// Add framework services.
builder.Services.AddMvc();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapHangfireDashboard();
app.MapDefaultControllerRoute();
app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");


BackgroundJob.Enqueue<BirdPiBackupHangFireWrapper>(x => x.Run(null));

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

public class BirdPiBackupHangFireWrapper
{
    private readonly WorkshopSettings _settings;

    public BirdPiBackupHangFireWrapper(IConfiguration configuration)
    {
        _settings = configuration.GetSection("WorkshopSettings").Get<WorkshopSettings>();
    }

    public async Task Run(PerformContext context)
    {
        await BirdPiBackup.Run(_settings, new Progress<string>(context.WriteLine));
    }
}