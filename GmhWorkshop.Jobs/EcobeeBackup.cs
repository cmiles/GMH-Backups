using Microsoft.Playwright;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace GmhWorkshop.Jobs;

public class EcobeeBackup
{
    public static async Task Run(WorkshopSettings settings, IProgress<string> progress)
    {
        var exitCode = Program.Main(new[] { "install" });

        if (exitCode != 0)
        {
            Log.ForContext("hint",
                    "Playwright requires binaries for the browsers - the Ats Buying Notes calls the Playwright install routine each time it runs both to make sure they are installed and also to keep on latest. This failure means there was an install problem that may need to be investigated - the program will continue to try to run since the needed binaries may already be present and the error could be transient. See the Playwright install docs for details including a PowerShell script you can run to do the install (included with this program).")
                .Warning(
                    "EcobeeBackup - Trouble Running the Playwright Browser Install - Exit Code {exitCode} Continuing",
                    exitCode);
        }

        //2023-7-8: The Ecobee API seemed like a challenge for a fully automated no
        //user action scenario?
        Log.Information("Starting {jobName}", nameof(EcobeeBackup));

        if (string.IsNullOrWhiteSpace(settings.EcobeeEmail))
        {
            Log.Error("EcobeeEmail is Blank");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.EcobeePassword))
        {
            Log.Error("EcobeePassword is Blank");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.EcobeeBackupDirectory))
        {
            Log.Error("EcobeeBackupDirectory is Blank");
            return;
        }

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
        });
        var context = await browser.NewContextAsync();

        var page = await context.NewPageAsync();

        await page.GotoAsync("https://www.ecobee.com/en-us/");

        await Task.Delay(5000);

        await page.HoverAsync("text=Sign In");

        await page.GetByRole(AriaRole.Link, new() { Name = "Sign in to your account" }).ClickAsync();

        await page.GetByLabel("Email").FillAsync(settings.EcobeeEmail);

        await page.GetByLabel("Password").FillAsync(settings.EcobeePassword);

        await (await page.GetByText("Sign in").AllAsync()).Last().ClickAsync();

        await Task.Delay(5000);

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.GotoAsync(
            "https://www.ecobee.com/consumerportal/index.html#/devices/thermostats/521739387948/homeiq/diagnostics/downloadData");

        var csvDownload = await page.RunAndWaitForDownloadAsync(async () =>
        {
            await page.GetByText("Download Last 7 Day's Data").ClickAsync();
            //await page.GetByRole(AriaRole.Button, new() { Name = "Download Last 7 Day's Data" }).ClickAsync();
        });

        var backupDirectory = new DirectoryInfo(settings.EcobeeBackupDirectory);

        if (!backupDirectory.Exists) backupDirectory.Create();


        var tempCsvFiles = new FileInfo(Path.Combine(backupDirectory.FullName,
            $"Temp-Ecobee-Csv-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.csv"));

        await csvDownload.SaveAsAsync(tempCsvFiles.FullName);

        var lines = await File.ReadAllLinesAsync(tempCsvFiles.FullName);

        var dateLines = new List<(DateOnly date, TimeOnly time, string line)>();
        var incompleteDates = new List<DateOnly>();
        var ecobeeId = lines[0].Replace("#,Thermostat,identifier,", "");
        var header = lines[5];

        foreach (var loopLine in lines.Skip(6))
        {
            var fields = loopLine.Split(',');

            //Depending on the file request Ecobee will send rows without information (or without
            //full information - skip these based on the System Mode having a value
            var date = DateOnly.Parse(fields[0]);

            if (incompleteDates.Contains(date)) continue;

            if (string.IsNullOrEmpty(fields[2]))
            {
                incompleteDates.Add(date);
                continue;
            }

            var time = TimeOnly.Parse(fields[1]);

            dateLines.Add((date, time, loopLine));
        }

        var dayGroups = dateLines.GroupBy(x => x.date);

        foreach (var dayGroup in dayGroups)
        {
            if (incompleteDates.Contains(dayGroup.Key)) continue;
            //Partial Day - skip
            if (dayGroup.Count() != 288) continue;

            var dayFile =
                new FileInfo(Path.Combine(backupDirectory.FullName, $"Ecobee-{dayGroup.Key:yyyy-MM-dd}-Backup.csv"));

            if (dayFile.Exists)
            {
                continue;
            }

            var fileText = header + Environment.NewLine + string.Join(Environment.NewLine,
                dayGroup.OrderBy(x => x.date).ThenBy(x => x.time).Select(x => x.line));

            await File.WriteAllTextAsync(dayFile.FullName, fileText);
        }

        Log.Information("Finished {jobName}", "EcobeeBackup");
    }
}