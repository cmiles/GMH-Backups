using GmhWorkshop.Jobs;
using Hangfire.Console;
using Hangfire.Server;
using Serilog;

namespace GmhWorkshop.Web.TaskWrappers;

public class SensorPushDayFileBackupHangfireWrapper
{
    private readonly WorkshopSettings? _settings;

    public SensorPushDayFileBackupHangfireWrapper(IConfiguration configuration)
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
        await SensorPushBackup.Run(_settings, new Progress<string>(context.WriteLine));
    }
}