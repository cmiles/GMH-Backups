using System.Text.Json;
using GmhWorkshop.CommonTools;
using GmhWorkshop.SensorPush;
using GmhWorkshop.SensorPushBackup;
using Serilog;
using Serilog.Enrichers.CallerInfo;
using Serilog.Events;
using Sensor = GmhWorkshop.SensorPush.Sensor;
using SensorPushCommonTools = GmhWorkshop.SensorPush.SensorPushCommonTools;
using SensorsRequest = GmhWorkshop.SensorPush.SensorsRequest;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .Enrich.WithCallerInfo(true,
        "GmhWorkshop.",
        "gmhworkshop")
    .WriteTo.Console(LogEventLevel.Verbose)
    .CreateLogger();

AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
{
    Log.Fatal(eventArgs.ExceptionObject as Exception,
        $"Unhandled Exception {(eventArgs.ExceptionObject as Exception)?.Message ?? ""}");
    Log.CloseAndFlush();
    return;
};

if (args.Length != 1)
{
    Log.Error(
        $"The Settings File must be provided as the only argument to this program (found {args.Count()} arguments)");
    await Log.CloseAndFlushAsync();
    return;
}

var settingsFileFromCommandline = args[0];

var settingFileReadAndSetup = new ObfuscatedSettingsConsoleSetup<SensorPushBackupSettings>()
{
    SettingsFile = settingsFileFromCommandline,
    SettingsFileIdentifier = SensorPushBackupSettings.SettingsTypeIdentifier,
    VaultServiceIdentifier = "http://sensorpushbackup.com",
    SettingsFileProperties =
    [
        new SettingsFileProperty<SensorPushBackupSettings>
        {
            PropertyDisplayName = "Email",
            PropertyEntryHelp =
                "The email to use to login to the SensorPush API.",
            HideEnteredValue = false,
            PropertyIsValid = PropertyIsValidIfNotNullOrWhiteSpace<SensorPushBackupSettings>(x => x.SensorPushEmail),
            UserEntryIsValid = UserEntryIsValidIfNotNullOrWhiteSpace(),
            SetValue = (settings, userEntry) => settings.SensorPushEmail = userEntry.Trim()
        },
        new SettingsFileProperty<SensorPushBackupSettings>
        {
            PropertyDisplayName = "Password",
            PropertyEntryHelp =
                "The password to use to login to the SensorPush API.",
            HideEnteredValue = true,
            PropertyIsValid = PropertyIsValidIfNotNullOrWhiteSpace<SensorPushBackupSettings>(x => x.SensorPushPassword),
            UserEntryIsValid = UserEntryIsValidIfNotNullOrWhiteSpace(),
            SetValue = (settings, userEntry) => settings.SensorPushPassword = userEntry.Trim()
        },
        new SettingsFileProperty<SensorPushBackupSettings>
        {
            PropertyDisplayName = "Backup Directory",
            PropertyEntryHelp =
                "The backup directory will be used to save the backup data - it is best to dedicate a directory just to this data to avoid conflicts with other data.",
            HideEnteredValue = false,
            PropertyIsValid =
                PropertyIsValidIfNotNullOrWhiteSpace<SensorPushBackupSettings>(x => x.SensorPushBackupDirectory),
            UserEntryIsValid = UserEntryIsValidIfNotNullOrWhiteSpace(),
            SetValue = (settings, userEntry) => settings.SensorPushBackupDirectory = userEntry.Trim()
        },
        new SettingsFileProperty<SensorPushBackupSettings>
        {
            PropertyDisplayName = "Days Back",
            PropertyEntryHelp =
                "The number of days back to check for backups.",
            HideEnteredValue = false,
            PropertyIsValid = PropertyIsValidIfPositiveInt<SensorPushBackupSettings>(x => x.SensorPushDaysBack),
            UserEntryIsValid = UserEntryIsValidIfInt(),
            SetValue = (settings, userEntry) => settings.SensorPushDaysBack = int.Parse(userEntry)
        }
    ]
};

var settingsSetupResult = await settingFileReadAndSetup.Setup();

if (!settingsSetupResult.isValid)
{
    return;
}

var settings = settingsSetupResult.settings;

var backupDirectory = new DirectoryInfo(settings.SensorPushBackupDirectory);
if (!backupDirectory.Exists)
{
    backupDirectory.Create();
}

//Create the Year Backup Directories
var startDay =
    DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)
        .AddDays(-1 * Math.Abs(settings.SensorPushDaysBack)));
var endDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1).Date);

Log.Debug($"Start Day UTC {startDay}, End Day UTC {endDay}");

var years = Enumerable.Range(startDay.Year, endDay.Year - startDay.Year + 1).ToList();

foreach (var loopYear in years)
{
    var loopYearDirectory = new DirectoryInfo(Path.Combine(settings.SensorPushBackupDirectory, loopYear.ToString()));
    if (!loopYearDirectory.Exists)
    {
        loopYearDirectory.Create();
    }
}

var client =
    await SensorPushCommonTools.AuthorizedClient(settings.SensorPushEmail, settings.SensorPushPassword);

var sensors = await client.SensorsAsync(new SensorsRequest());

var allPossibleBackupDays = Enumerable.Range(0, 1 + (endDay.DayNumber - startDay.DayNumber))
    .Select(offset => startDay.AddDays(offset))
    .ToList();

var devicesAndDaysToDownload = new List<(Sensor device, DateOnly date)>();

var dayProgressCounter = 0;

foreach (var loopDays in allPossibleBackupDays)
{
    dayProgressCounter++;

    var directory = new DirectoryInfo(Path.Combine(backupDirectory.FullName, loopDays.Year.ToString()));

    foreach (var loopSensor in sensors)
    {
        var existingFiles =
            directory.EnumerateFiles($"SensorPush-*-{loopSensor.Key}-{loopDays:yyyy-MM-dd}.json");

        if (existingFiles.Any())
        {
            Log.Verbose(
                "Backup for {startDate} already exists - skipping. {progressCount} of {totalCount}.",
                loopDays, dayProgressCounter, allPossibleBackupDays.Count);
        }

        devicesAndDaysToDownload.Add((loopSensor.Value, loopDays));
    }
}

Log.Debug($"Found {devicesAndDaysToDownload.Count} Days/Devices to Download");

var fileCounter = 0;

devicesAndDaysToDownload =
    devicesAndDaysToDownload.OrderBy(x => x.device.Name).ThenBy(x => x.date).ToList();

foreach (var loopDays in devicesAndDaysToDownload)
{
    fileCounter++;

    var samples = await client.SamplesAsync(new SamplesRequest
    {
        Sensors = loopDays.device.Id.AsList(), Active = true,
        //7/3/2023 - It appears that the baseline is one reading per second - the 99999 value is just for 
        //safety to try to adjust if once per second changes. Also the API specifies that at a certain
        //size (5mb) the request will be truncated.
        Limit = 99999,
        StartTime =
            loopDays.date.ToDateTime(new TimeOnly()).AddMinutes(-30).ToString("yyyy-MM-ddTHH:MM:ss-0000"),
        StopTime = loopDays.date.ToDateTime(new TimeOnly()).AddDays(1).AddMinutes(30)
            .ToString("yyyy-MM-ddTHH:MM:ss-0000")
    });

    if (samples?.Sensors?.FirstOrDefault() is null || !samples.Sensors.Any() ||
        !samples.Sensors.First().Value.Any())
    {
        Log.Warning(
            "Failed to write Backup Files for {startDate} - {deviceName} - {deviceId} - No Samples Found",
            loopDays.date, loopDays.device.Name, loopDays.device.DeviceId);
        continue;
    }

    if (samples.Truncated ?? false)
    {
        //Todo: this may represent some kind of API change and needs a user alert
        Log
            .ForContext("hint", "SensorPush API calls have a 'truncated' property and with large " +
                                "requests (over 5mb according to the documentation) may mark the " +
                                "request truncated so that a client could detect this and then " +
                                "figure out how to retrieve the needed additional data. However in " +
                                "this routine - single sensor, single day - at least with current " +
                                "sensors this probably indicates something has gone wrong... Testing " +
                                "with the API in July of 2023 indicated one reading per minute which " +
                                "should never be truncated...")
            .ForContext(nameof(loopDays), loopDays, true)
            .ForContext(nameof(samples.Last_time), samples.Last_time)
            .ForContext(nameof(samples.Status), samples.Status)
            .ForContext(nameof(samples.Total_samples), samples.Total_samples)
            .ForContext(nameof(samples.Total_sensors), samples.Total_sensors)
            .ForContext(nameof(samples.Truncated), samples.Truncated)
            .Error(
                "Failed to write Backup Files for {startDate} - {deviceName} - {deviceId} - Found more Samples than the API can transmit in a single call?",
                loopDays.date, loopDays.device.Name, loopDays.device.DeviceId);
        continue;
    }

    var dayJsonFile = new FileInfo(Path.Combine(backupDirectory.FullName,
        loopDays.date.Year.ToString(),
        $"SensorPush-{loopDays.device.Name}-{loopDays.device.DeviceId}-{loopDays.date:yyyy-MM-dd}.json"));

    var wroteTransformed = false;
    var wroteApi = false;

    try
    {
        if (dayJsonFile.Exists)
        {
            Log.Verbose(
                "API Json File {fileName} already exists - skipping. {progressCount} of {totalCount}.",
                dayJsonFile.FullName, fileCounter, devicesAndDaysToDownload.Count);
        }
        else
        {
            var daySamples = samples.Sensors.First().Value
                .Where(x => DateOnly.FromDateTime(x.Observed!.Value.UtcDateTime) == loopDays.date)
                .OrderBy(x => x.Observed).ToList();

            var sampleFileData = new SensorPushBackupDay
            {
                Date = loopDays.date,
                DeviceName = loopDays.device.Name,
                Samples = daySamples
            };

            await File.WriteAllTextAsync(dayJsonFile.FullName,
                JsonSerializer.Serialize(sampleFileData,
                    new JsonSerializerOptions { WriteIndented = true }));
            wroteApi = true;
        }
    }
    catch (Exception e)
    {
        Log.ForContext("hint", "This error is skipped and execution continues - because this method scans" +
                               "at least one previous month the assumption is that there will be multiple " +
                               "attempts at writing this date (this method is best used daily or weekly) and " +
                               "that file system errors are likely to be transient.")
            .ForContext("exception", e, true)
            .ForContext(nameof(loopDays), loopDays, true)
            .ForContext(nameof(samples.Last_time), samples.Last_time)
            .ForContext(nameof(samples.Status), samples.Status)
            .ForContext(nameof(samples.Total_samples), samples.Total_samples)
            .ForContext(nameof(samples.Total_sensors), samples.Total_sensors)
            .ForContext(nameof(samples.Truncated), samples.Truncated)
            .ForContext(nameof(wroteTransformed), wroteTransformed)
            .ForContext(nameof(dayJsonFile), dayJsonFile.FullName)
            .ForContext(nameof(wroteApi), wroteApi).ForContext("dayApiJsonFile", dayJsonFile.FullName)
            .Warning("Failed to write Backup Files for {startDate} - {deviceName} - {deviceId}",
                loopDays.date, loopDays.device.Name, loopDays.device.DeviceId);
        continue;
    }

    Log.ForContext(nameof(wroteTransformed), wroteTransformed)
        .ForContext(nameof(dayJsonFile), dayJsonFile.FullName)
        .ForContext(nameof(wroteApi), wroteApi)
        .Verbose(
            "Backup for {startDate} - {deviceName} - {deviceId} - written to {backupFileDirectory}. {progressCount} of {totalCount}.",
            loopDays.date, loopDays.device.Name, loopDays.device.DeviceId, dayJsonFile.Directory?.FullName,
            fileCounter,
            devicesAndDaysToDownload.Count);
}

Log.Information("Finished {jobName}", "SensorPushWeatherBackup");

Func<T, bool> ShouldSetPropertyIfNullOrWhiteSpace<T>(Func<T, string> propertySelector)
{
    return settings =>
    {
        if (string.IsNullOrWhiteSpace(propertySelector(settings)))
        {
            return true;
        }

        return false;
    };
}

Func<T, (bool isValid, string message)> PropertyIsValidIfNotNullOrWhiteSpace<T>(Func<T, string> propertySelector)
{
    return settings =>
    {
        if (string.IsNullOrWhiteSpace(propertySelector(settings)))
        {
            return (false, "Value can not be blank.");
        }

        return (true, string.Empty);
    };
}

Func<T, (bool isValid, string message)> PropertyIsValidIfPositiveInt<T>(Func<T, int> propertySelector)
{
    return backupSettings =>
    {
        if (propertySelector(backupSettings) < 1)
        {
            return (false, "Value must be a positive number.");
        }

        return (true, string.Empty);
    };
}

Func<string, (bool isValid, string message)> UserEntryIsValidIfNotNullOrWhiteSpace()
{
    return userEntry =>
    {
        if (string.IsNullOrWhiteSpace(userEntry))
        {
            return (false, "The value can not be blank.");
        }

        return (true, string.Empty);
    };
}

Func<string, (bool isValid, string message)> UserEntryIsValidIfInt()
{
    return userEntry =>
    {
        if (string.IsNullOrWhiteSpace(userEntry))
        {
            return (false, "The value can not be blank.");
        }

        if (!int.TryParse(userEntry, out _))
        {
            return (false, "The value must be a number.");
        }

        return (true, string.Empty);
    };
}