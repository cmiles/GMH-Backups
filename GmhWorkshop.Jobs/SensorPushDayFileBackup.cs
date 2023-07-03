using System.Text.Json;
using GmhWorkshop.CommonTools;
using GmhWorkshop.SensorPush;
using Serilog;

namespace GmhWorkshop.Jobs
{
    public static class SensorPushDayFileBackup
    {
        public static async Task Run(WorkshopSettings settings, IProgress<string> progress)
        {
            Log.Information("Starting {jobName}", "TempestWeatherBackup");

            if (string.IsNullOrWhiteSpace(settings.SensorPushEmail))
            {
                Log.Error("SensorPushEmail is Blank");
                return;
            }

            if (string.IsNullOrWhiteSpace(settings.SensorPushBackupDirectory))
            {
                Log.Error("PushSensorBackupDirectory is Blank");
                return;
            }

            if (string.IsNullOrWhiteSpace(settings.SensorPushPassword))
            {
                Log.Error("SensorPushPassword is Blank");
                return;
            }

            if (settings.SensorPushMonthsBack < 1)
            {
                Log.Error("PushSensorMonthsBack Must be 1 or more");
                return;
            }

            var backupDirectory = new DirectoryInfo(settings.SensorPushBackupDirectory);

            if (!backupDirectory.Exists)
            {
                backupDirectory.Create();
            }

            //Create the Year Backup Directories
            var startDay =
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)
                    .AddMonths(-1 * Math.Abs(settings.SensorPushMonthsBack)));
            var endDay = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1).Date);

            progress.Report($"Start Day UTC {startDay}, End Day UTC {endDay}");

            var years = Enumerable.Range(startDay.Year, endDay.Year - startDay.Year + 1).ToList();

            foreach (var loopYear in years)
            {
                var loopYearDirectory = new DirectoryInfo(Path.Combine(backupDirectory.FullName, loopYear.ToString()));
                if (!loopYearDirectory.Exists)
                {
                    loopYearDirectory.Create();
                }
            }

            var authClient = new Client(new HttpClient());
            var authR = await authClient.AuthorizeAsync(new AuthorizeRequest
                { Email = settings.SensorPushEmail, Password = settings.SensorPushPassword });
            var oAuth = await authClient.AccessTokenAsync(
                new AccessTokenRequest { Authorization = authR.Authorization });

            var httpClientWithAuthorization = new HttpClient();
            httpClientWithAuthorization.DefaultRequestHeaders.Add("Authorization", oAuth.Accesstoken);

            var client = new Client(httpClientWithAuthorization);

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

            progress.Report($"Found {devicesAndDaysToDownload.Count} Days/Devices to Download");

            var fileCounter = 0;

            devicesAndDaysToDownload =
                devicesAndDaysToDownload.OrderBy(x => x.device.Name).ThenBy(x => x.date).ToList();

            foreach (var loopDays in devicesAndDaysToDownload)
            {
                fileCounter++;

                var samples = await client.SamplesAsync(new SamplesRequest
                {
                    Sensors = loopDays.device.Id.AsList(), Active = true,
                    Limit = 2000,
                    StartTime =
                        loopDays.date.ToDateTime(new TimeOnly()).AddMinutes(-30).ToString("yyyy-MM-ddTHH:MM:ss-0000"),
                    StopTime = loopDays.date.ToDateTime(new TimeOnly()).AddDays(1).AddMinutes(30)
                        .ToString("yyyy-MM-ddTHH:MM:ss-0000")
                });

                if (samples?.Sensors?.FirstOrDefault() == null || !samples.Sensors.Any() ||
                    !samples.Sensors.First().Value.Any())
                {
                    Log.Warning(
                        "Failed to write Backup Files for {startDate} - {deviceName} - {deviceId} - No Samples Found",
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

                        await File.WriteAllTextAsync(dayJsonFile.FullName,
                            JsonSerializer.Serialize((loopDays.device, daySamples),
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
        }
    }
}