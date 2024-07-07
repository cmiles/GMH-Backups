using System.Text.Json;
using GmhWorkshop.TempestWeatherBackup;
using GmhWorkshop.WeatherFlowTempest;
using Microsoft.Extensions.Logging;
using PointlessWaymarks.CommonTools;
using PointlessWaymarks.VaultfuscationTools;
using Serilog;
using Serilog.Enrichers.CallerInfo;
using Serilog.Extensions.Logging;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .Enrich.WithCallerInfo(true,
        "GmhWorkshop.",
        "gmhworkshop")
    .CreateLogger();

AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
{
    Log.Fatal(eventArgs.ExceptionObject as Exception,
        $"Unhandled Exception {(eventArgs.ExceptionObject as Exception)?.Message ?? ""}");
    Log.CloseAndFlush();
};

if (args.Length != 1)
{
    Log.Error(
        $"The Settings File must be provided as the only argument to this program (found {args.Length} arguments)");
    await Log.CloseAndFlushAsync();
    return;
}

var cleanedSettingsFile = args[0].Trim();

var interactive = !args.Any(x => x.Contains("-notinteractive", StringComparison.OrdinalIgnoreCase));
var promptAsIfNewFile = args.Any(x => x.Contains("-redo", StringComparison.OrdinalIgnoreCase));

var msLogger = new SerilogLoggerFactory(Log.Logger)
    .CreateLogger<ObfuscatedSettingsConsoleSetup<TempestWeatherBackupSettings>>();

var settingFileReadAndSetup = new ObfuscatedSettingsConsoleSetup<TempestWeatherBackupSettings>(msLogger)
{
    SettingsFile = cleanedSettingsFile,
    SettingsFileIdentifier = TempestWeatherBackupSettings.SettingsTypeIdentifier,
    VaultServiceIdentifier = "http://tempestweatherbackup.private",
    Interactive = interactive,
    PromptAsIfNewFile = promptAsIfNewFile,
    SettingsFileProperties =
    [
        new SettingsFileProperty<TempestWeatherBackupSettings>
        {
            PropertyDisplayName = "Tempest Access Token",
            PropertyEntryHelp =
                "The token used to login to the Tempest WeatherFlow API.",
            HideEnteredValue = true,
            PropertyIsValid =
                ObfuscatedSettingsHelpers.PropertyIsValidIfNotNullOrWhiteSpace<TempestWeatherBackupSettings>(x =>
                    x.TempestAccessToken),
            UserEntryIsValid = ObfuscatedSettingsHelpers.UserEntryIsValidIfNotNullOrWhiteSpace(),
            SetValue = (settings, userEntry) => settings.TempestAccessToken = userEntry.Trim()
        },
        new SettingsFileProperty<TempestWeatherBackupSettings>
        {
            PropertyDisplayName = "Device Id",
            PropertyEntryHelp =
                "The device to Backup.",
            HideEnteredValue = false,
            PropertyIsValid =
                ObfuscatedSettingsHelpers.PropertyIsValidIfPositiveInt<TempestWeatherBackupSettings>(x =>
                    x.TempestDeviceId),
            UserEntryIsValid = ObfuscatedSettingsHelpers.UserEntryIsValidIfInt(),
            SetValue = (settings, userEntry) => settings.TempestDeviceId = int.Parse(userEntry)
        },
        new SettingsFileProperty<TempestWeatherBackupSettings>
        {
            PropertyDisplayName = "Backup Directory",
            PropertyEntryHelp =
                "The backup directory will be used to save the backup data - it is best to dedicate a directory just to this data to avoid conflicts with other data.",
            HideEnteredValue = false,
            PropertyIsValid =
                ObfuscatedSettingsHelpers.PropertyIsValidIfNotNullOrWhiteSpace<TempestWeatherBackupSettings>(x =>
                    x.TempestFileBackupDirectory),
            UserEntryIsValid = ObfuscatedSettingsHelpers.UserEntryIsValidIfNotNullOrWhiteSpace(),
            SetValue = (settings, userEntry) => settings.TempestFileBackupDirectory = userEntry.Trim()
        },
        new SettingsFileProperty<TempestWeatherBackupSettings>
        {
            PropertyDisplayName = "Days Back",
            PropertyEntryHelp =
                "The number of days back to check for backups.",
            HideEnteredValue = false,
            PropertyIsValid =
                ObfuscatedSettingsHelpers.PropertyIsValidIfPositiveInt<TempestWeatherBackupSettings>(x =>
                    x.TempestDaysBack),
            UserEntryIsValid = ObfuscatedSettingsHelpers.UserEntryIsValidIfInt(),
            SetValue = (settings, userEntry) => settings.TempestDaysBack = int.Parse(userEntry)
        }
    ]
};

var settingsSetupResult = await settingFileReadAndSetup.Setup();

if (!settingsSetupResult.isValid)
{
    return;
}

var settings = settingsSetupResult.settings;

var backupDirectory = new DirectoryInfo(settings.TempestFileBackupDirectory);

if (!backupDirectory.Exists)
{
    backupDirectory.Create();
}

//Create the Year Backup Directories
var startDay =
    DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1).AddDays(-1 * Math.Abs(settings.TempestDaysBack)));
var endDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1).Date);

Log.Debug($"Start Day UTC {startDay}, End Day UTC {endDay}");

var years = Enumerable.Range(startDay.Year, endDay.Year - startDay.Year + 1).ToList();

foreach (var loopYear in years)
{
    var loopYearDirectory = new DirectoryInfo(Path.Combine(backupDirectory.FullName, loopYear.ToString()));
    if (!loopYearDirectory.Exists)
    {
        loopYearDirectory.Create();
    }
}

var allPossibleBackupDays = Enumerable.Range(0, 1 + (endDay.DayNumber - startDay.DayNumber))
    .Select(offset => startDay.AddDays(offset))
    .ToList();

var daysToDownload = new List<DateOnly>();

var dayProgressCounter = 0;

foreach (var loopDays in allPossibleBackupDays)
{
    dayProgressCounter++;

    var dayJsonTransformedFile = new FileInfo(Path.Combine(backupDirectory.FullName,
        loopDays.Year.ToString(),
        $"TempestWeatherStation-{settings.TempestDeviceId}-{loopDays:yyyy-MM-dd}-Transformed.json"));

    var dayApiJsonFile = new FileInfo(Path.Combine(backupDirectory.FullName, loopDays.Year.ToString(),
        $"TempestWeatherStation-{settings.TempestDeviceId}-{loopDays:yyyy-MM-dd}-ApiJson.json"));

    if (dayJsonTransformedFile.Exists && dayApiJsonFile.Exists)
    {
        Log.Verbose("Backup for {startDate} already exists - skipping. {progressCount} of {totalCount}.",
            loopDays, dayProgressCounter, allPossibleBackupDays.Count);
    }

    daysToDownload.Add(loopDays);
}

Log.Debug($"Found {daysToDownload.Count} Days to Download");

var downloads = await Observations.GetObservations(settings.TempestDeviceId,
    settings.TempestAccessToken,
    daysToDownload, new ConsoleProgress());

var fileCounter = 0;

foreach (var loopDownloads in downloads)
{
    fileCounter++;

    var dayTransformedJsonFile = new FileInfo(Path.Combine(backupDirectory.FullName,
        loopDownloads.observation.UtcDate.Year.ToString(),
        $"TempestWeatherStation-{settings.TempestDeviceId}-{loopDownloads.observation.UtcDate:yyyy-MM-dd}-Transformed.json"));

    var dayApiJsonFile = new FileInfo(Path.Combine(backupDirectory.FullName,
        loopDownloads.observation.UtcDate.Year.ToString(),
        $"TempestWeatherStation-{settings.TempestDeviceId}-{loopDownloads.observation.UtcDate:yyyy-MM-dd}-ApiJson.json"));

    var wroteTransformed = false;
    var wroteApi = false;

    try
    {
        if (dayTransformedJsonFile.Exists)
        {
            Log.Verbose(
                "Transformed Json File {fileName} already exists - skipping. {progressCount} of {totalCount}.",
                dayTransformedJsonFile.FullName, fileCounter, downloads.Count);
        }
        else
        {
            await File.WriteAllTextAsync(dayTransformedJsonFile.FullName,
                JsonSerializer.Serialize(loopDownloads.observation,
                    new JsonSerializerOptions { WriteIndented = true }));
            wroteTransformed = true;
        }

        if (dayApiJsonFile.Exists)
        {
            Log.Verbose(
                "API Json File {fileName} already exists - skipping. {progressCount} of {totalCount}.",
                dayApiJsonFile.FullName, fileCounter, downloads.Count);
        }
        else
        {
            await File.WriteAllTextAsync(dayApiJsonFile.FullName, loopDownloads.observationsJson);
            wroteApi = true;
        }
    }
    catch (Exception e)
    {
        Log.ForContext("hint", "This error is skipped and execution continues - because this method scans " +
                               "at least one previous month the assumption is that there will be multiple " +
                               "attempts at writing this date (this method is best used daily or weekly) and " +
                               "that file system errors are likely to be transient.")
            .ForContext("exception", e, true)
            .ForContext("wroteTransformed", wroteTransformed)
            .ForContext("dayTransformedJsonFile", dayTransformedJsonFile.FullName)
            .ForContext("wroteApi", wroteApi).ForContext("dayApiJsonFile", dayApiJsonFile.FullName)
            .Warning("Failed to write Backup Files for {startDate}", loopDownloads.observation.UtcDate);
        continue;
    }

    Log.ForContext("wroteTransformed", wroteTransformed)
        .ForContext("dayTransformedJsonFile", dayTransformedJsonFile.FullName)
        .ForContext("wroteApi", wroteApi).ForContext("dayApiJsonFile", dayApiJsonFile.FullName).Verbose(
            "Backup for {startDate} written to {backupFileDirectory}. {progressCount} of {totalCount}.",
            loopDownloads.observation.UtcDate, dayApiJsonFile.Directory?.FullName, fileCounter,
            allPossibleBackupDays.Count);
}

Log.Information("Finished {jobName}", "TempestWeatherBackup");