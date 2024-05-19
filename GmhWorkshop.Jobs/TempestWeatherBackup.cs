using System.Text.Json;
using GmhWorkshop.WeatherFlowTempest;
using Serilog;

namespace GmhWorkshop.Jobs;

/// <summary>
///     Pro
/// </summary>
public static class TempestWeatherBackup
{
    public static async Task Run(WorkshopSettings settings, IProgress<string> progress)
    {
        Log.Information("Starting {jobName}", nameof(TempestWeatherBackup));

        if (string.IsNullOrWhiteSpace(settings.TempestAccessToken))
        {
            Log.Error("TempestAccessToken is Blank");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.TempestFileBackupDirectory))
        {
            Log.Error("TempestFileBackupDirectory is Blank");
            return;
        }

        if (settings.TempestMonthsBack < 1)
        {
            Log.Error("TempestMonthsBack Must be 1 or more");
            return;
        }

        if (settings.TempestDeviceId < 1)
        {
            Log.Error("TempestDeviceId does not appear to be valid");
            return;
        }

        var backupDirectory = new DirectoryInfo(settings.TempestFileBackupDirectory);

        if (!backupDirectory.Exists)
        {
            backupDirectory.Create();
        }

        //Create the Year Backup Directories
        var startDay =
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1).AddMonths(-1 * Math.Abs(settings.TempestMonthsBack)));
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

        progress.Report($"Found {daysToDownload.Count} Days to Download");

        var downloads = await Observations.GetObservations(settings.TempestDeviceId,
            settings.TempestAccessToken,
            daysToDownload, progress);

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
                Log.ForContext("hint", "This error is skipped and execution continues - because this method scans" +
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
    }
}