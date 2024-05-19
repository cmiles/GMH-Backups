using Microsoft.Playwright;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GmhWorkshop.VictronRemoteMonitoring;

namespace GmhWorkshop.Jobs;

public static class VictronRemoteMonitoringBackup
{
    public static async Task Run(WorkshopSettings settings, IProgress<string> progress)
    {
        var exitCode = Program.Main(new[] { "install" });

        Log.Information("Starting {jobName}", nameof(VictronRemoteMonitoringBackup));

        if (string.IsNullOrWhiteSpace(settings.TepEmail))
        {
            Log.Error("VrmEmail is Blank");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.TepPassword))
        {
            Log.Error("VrmPassword is Blank");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.TepBackupDirectory))
        {
            Log.Error("VrmBackupDirectory is Blank");
            return;
        }

        var backupDirectory = new DirectoryInfo(settings.TepBackupDirectory);
        if (!backupDirectory.Exists)
        {
            backupDirectory.Create();
        }

        progress.Report("Starting Authorization");

        var authResponse = await VictronVrmTools.Login(settings.TepEmail, settings.TepPassword);
        var user = await VictronVrmTools.GetLoggedInUser(authResponse.token);
        var installs = await VictronVrmTools.GetInstallations(authResponse.token, user.user.id);

        foreach (var loopInstalls in installs.records)
        {
            var name = loopInstalls.name;
        }
    }
}