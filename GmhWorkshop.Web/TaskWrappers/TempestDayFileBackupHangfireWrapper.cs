using GmhWorkshop.Jobs;
using Hangfire.Console;
using Hangfire.Server;
using Serilog;

namespace GmhWorkshop.Web.TaskWrappers;

public class TempestDayFileBackupHangfireWrapper
{
    private readonly WorkshopSettings? _settings;

    public TempestDayFileBackupHangfireWrapper(IConfiguration configuration)
    {
        _settings = configuration.GetSection("WorkshopSettings").Get<WorkshopSettings>();
    }

    public async Task Run(PerformContext context)
    {
        if (_settings is null)
        {
            Log.ForContext("hint", "Settings are taken from the IConfiguration passed to the " +
                                   "constructor - a null could indicate the apps secrets are " +
                                   "not setup correctly or a DI error where the configuration is " +
                                   "not initialized/present/available?")
                .Error("{wrapperName} received null settings?", nameof(SensorPushDayFileBackupHangfireWrapper));
            return;
        }
        await TempestWeatherDayFileBackup.Run(_settings, new Progress<string>(context.WriteLine));
    }
}