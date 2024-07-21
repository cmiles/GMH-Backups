using System.Text.Json;
using GmhWorkshop.CommonTools;
using GmhWorkshop.VictronRemoteMonitoring;
using GmhWorkshop.VictronRemoteMonitoring.Models;
using GmhWorkshop.VictronRemoteMonitoringBackup;
using Microsoft.Extensions.Logging;
using PointlessWaymarks.CommonTools.S3;
using PointlessWaymarks.VaultfuscationTools;
using Serilog;
using Serilog.Extensions.Logging;

var parsedSettings = await SetupTools.SetupAndGetSettingsFile(args);

if (string.IsNullOrWhiteSpace(parsedSettings.SettingsFile))
{
    return -1;
}

var msLogger = new SerilogLoggerFactory(Log.Logger)
    .CreateLogger<ObfuscatedSettingsConsoleSetup<VictronRemoteMonitoringBackupSettings>>();

var settingFileReadAndSetup = new ObfuscatedSettingsConsoleSetup<VictronRemoteMonitoringBackupSettings>(msLogger)
{
    SettingsFile = parsedSettings.SettingsFile,
    SettingsFileIdentifier = VictronRemoteMonitoringBackupSettings.SettingsTypeIdentifier,
    VaultServiceIdentifier = "http://victronremotemonitoringbackup.private",
    Interactive = parsedSettings.Interactive,
    PromptAsIfNewFile = parsedSettings.PromptAsIfNewFile,
    SettingsFileProperties =
    [
        new SettingsFileProperty<VictronRemoteMonitoringBackupSettings>
        {
            PropertyDisplayName = "Email",
            PropertyEntryHelp =
                "The email to use to login for Victron Remote Monitoring.",
            HideEnteredValue = false,
            PropertyIsValid =
                ObfuscatedSettingsHelpers.PropertyIsValidIfNotNullOrWhiteSpace<VictronRemoteMonitoringBackupSettings>(
                    x =>
                        x.VrmEmail),
            UserEntryIsValid = ObfuscatedSettingsHelpers.UserEntryIsValidIfNotNullOrWhiteSpace(),
            SetValue = (settings, userEntry) => settings.VrmEmail = userEntry.Trim()
        },
        new SettingsFileProperty<VictronRemoteMonitoringBackupSettings>
        {
            PropertyDisplayName = "Password",
            PropertyEntryHelp =
                "The password to use to login for Victron Remote Monitoring.",
            HideEnteredValue = true,
            PropertyIsValid =
                ObfuscatedSettingsHelpers.PropertyIsValidIfNotNullOrWhiteSpace<VictronRemoteMonitoringBackupSettings>(
                    x =>
                        x.VrmPassword),
            UserEntryIsValid = ObfuscatedSettingsHelpers.UserEntryIsValidIfNotNullOrWhiteSpace(),
            SetValue = (settings, userEntry) => settings.VrmPassword = userEntry.Trim()
        },
        new SettingsFileProperty<VictronRemoteMonitoringBackupSettings>
        {
            PropertyDisplayName = "Backup Directory",
            PropertyEntryHelp =
                "The backup directory will be used to save the backup data - it is best to dedicate a directory just to this data to avoid conflicts with other data.",
            HideEnteredValue = false,
            PropertyIsValid =
                ObfuscatedSettingsHelpers.PropertyIsValidIfNotNullOrWhiteSpace<VictronRemoteMonitoringBackupSettings>(
                    x =>
                        x.VrmBackupDirectory),
            UserEntryIsValid = ObfuscatedSettingsHelpers.UserEntryIsValidIfNotNullOrWhiteSpace(),
            SetValue = (settings, userEntry) => settings.VrmBackupDirectory = userEntry.Trim()
        },
        new SettingsFileProperty<VictronRemoteMonitoringBackupSettings>
        {
            PropertyDisplayName = "Days Back",
            PropertyEntryHelp =
                "The number of days back to check for backups.",
            HideEnteredValue = false,
            PropertyIsValid =
                ObfuscatedSettingsHelpers.PropertyIsValidIfPositiveInt<VictronRemoteMonitoringBackupSettings>(x =>
                    x.VrmDaysBack),
            UserEntryIsValid = ObfuscatedSettingsHelpers.UserEntryIsValidIfInt(),
            SetValue = (settings, userEntry) => settings.VrmDaysBack = int.Parse(userEntry)
        }
    ]
};

var settingsSetupResult = await settingFileReadAndSetup.Setup();

if (!settingsSetupResult.isValid)
{
    return -1;
}

var settings = settingsSetupResult.settings;

var backupDirectory = new DirectoryInfo(settings.VrmBackupDirectory);

if (!backupDirectory.Exists)
{
    backupDirectory.Create();
}

//Create the Year Backup Directories
var startDay =
    DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)
        .AddDays(-1 * Math.Abs(settings.VrmDaysBack)));
var endDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1).Date);

Log.Debug($"Start Day UTC {startDay}, End Day UTC {endDay}");

var years = Enumerable.Range(startDay.Year, endDay.Year - startDay.Year + 1).ToList();

foreach (var loopYear in years)
{
    var loopYearDirectory = new DirectoryInfo(Path.Combine(settings.VrmBackupDirectory, loopYear.ToString()));
    if (!loopYearDirectory.Exists)
    {
        loopYearDirectory.Create();
    }
}

var allPossibleBackupDays = Enumerable.Range(0, 1 + (endDay.DayNumber - startDay.DayNumber))
    .Select(offset => startDay.AddDays(offset))
    .ToList();

var dayProgressCounter = 0;

var tokenResponse = await VictronVrmTools.Login(settings.VrmEmail, settings.VrmPassword);

var installs = await VictronVrmTools.Installations(tokenResponse!.token, tokenResponse!.idUser.ToString());

var errorMessages = new List<string>();

foreach (var loopDays in allPossibleBackupDays)
{
    Console.WriteLine($"Getting Stats for Day {++dayProgressCounter} of {allPossibleBackupDays.Count}");

    var yearDirectory = new DirectoryInfo(Path.Combine(backupDirectory.FullName, loopDays.Year.ToString()));

    var startUtc = loopDays.ToDateTime(new TimeOnly(0, 0));
    var endUtc = loopDays.ToDateTime(new TimeOnly(23, 59, 59));

    foreach (var install in installs!.records)
    {
        Console.WriteLine($"Getting Devices for Installation {install.name}");

        var installDevices = await VictronVrmTools.DeviceList(tokenResponse!.token, install.idSite.ToString());

        Console.WriteLine($"Getting Stats for Installation {install.name}");

        List<VrmStat> installationStats;

        try
        {
            installationStats = await VictronVrmTools.AllStatsFromDiagnostics(tokenResponse!.token,
                install.idSite.ToString(), startUtc,
                endUtc);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            errorMessages.Add(
                $"Error Getting Stats for Installation {install.name}, {install.idSite} - UTC Date Range: {startUtc} to {endUtc} - {e.Message}");
            continue;
        }

        var statsByInstance = installationStats.GroupBy(x => x.Instance).ToList();

        foreach (var loopInstance in statsByInstance)
        {
            var instanceStats = loopInstance.OrderBy(x => x.StatCode).ToList();

            Console.WriteLine(
                $"Instance {loopInstance.Key} - {instanceStats.Count} Stats");

            var instanceDevices = installDevices.records.devices.Where(x => x.instance == loopInstance.Key)
                .OrderBy(x => x.name).ToList();

            var installDirectory = new DirectoryInfo(Path.Combine(yearDirectory.FullName, install.name));

            if (!installDirectory.Exists)
            {
                installDirectory.Create();
            }

            var file = new FileInfo(Path.Combine(installDirectory.FullName,
                $"VRM-{install.identifier}-{loopInstance.Key}-{(instanceDevices.Any() ? $"{string.Join("_", instanceDevices.Select(x => x.name))}" : "")}--{loopDays.Year}-{loopDays.Month:D2}-{loopDays.Day:D2}.json"));

            if (file.Exists)
            {
                Console.WriteLine($"File {file.FullName} already exists - skipping");
                continue;
            }

            var toSave = new VrmInstallationStats
            {
                Installation = install,
                Stats = instanceStats,
                Device = instanceDevices
            };

            Console.WriteLine($"Saving Stats for Installation {install.name} - {file.FullName}");

            File.WriteAllText(file.FullName, JsonSerializer.Serialize(toSave, JsonTools.WriteIndentedOptions));
        }
    }

    await Task.Delay(30000);
}

if (errorMessages.Any())
{
    Console.WriteLine("Finished with Errors:");

    foreach (var loopError in errorMessages)
    {
        Console.WriteLine();
        Console.WriteLine(loopError);
    }

    return -1;
}

return 0;