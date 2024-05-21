using GmhWorkshop.BirdPiBackup;
using GmhWorkshop.CommonTools;
using Serilog.Events;
using Serilog;
using Serilog.Enrichers.CallerInfo;
using Renci.SshNet;
using System;

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

var settingFileReadAndSetup = new ObfuscatedSettingsConsoleSetup<BirdPiBackupSettings>()
{
    SettingsFile = settingsFileFromCommandline,
    SettingsFileIdentifier = BirdPiBackupSettings.SettingsTypeIdentifier,
    VaultServiceIdentifier = "http://birdpibackup.test",
    SettingsFileProperties =
    [
        new SettingsFileProperty<BirdPiBackupSettings>
        {
            PropertyDisplayName = "SFTP User Name",
            PropertyEntryHelp =
                "The User Name used to login via SFTP to the BirdPi.",
            HideEnteredValue = false,
            PropertyIsValid = ObfuscatedSettingsConsoleTools.PropertyIsValidIfNotNullOrWhiteSpace<BirdPiBackupSettings>(x => x.BirdPiSftpUser),
            UserEntryIsValid = ObfuscatedSettingsConsoleTools.UserEntryIsValidIfNotNullOrWhiteSpace(),
            SetValue = (settings, userEntry) => settings.BirdPiSftpUser = userEntry.Trim()
        },
        new SettingsFileProperty<BirdPiBackupSettings>
        {
            PropertyDisplayName = "SFTP Password",
            PropertyEntryHelp =
                "The SFTP password for the BirdPi.",
            HideEnteredValue = true,
            PropertyIsValid = ObfuscatedSettingsConsoleTools.PropertyIsValidIfNotNullOrWhiteSpace<BirdPiBackupSettings>(x => x.BirdPiSftpPassword),
            UserEntryIsValid = ObfuscatedSettingsConsoleTools.UserEntryIsValidIfNotNullOrWhiteSpace(),
            SetValue = (settings, userEntry) => settings.BirdPiSftpPassword = userEntry.Trim()
        },
        new SettingsFileProperty<BirdPiBackupSettings>
        {
            PropertyDisplayName = "Local Backup Directory",
            PropertyEntryHelp =
                "The backup directory will be used to save the backup data - it is best to dedicate a directory just to this data to avoid conflicts with other data.",
            HideEnteredValue = false,
            PropertyIsValid =
                ObfuscatedSettingsConsoleTools.PropertyIsValidIfNotNullOrWhiteSpace<BirdPiBackupSettings>(x => x.BirdPiBackupDirectory),
            UserEntryIsValid = ObfuscatedSettingsConsoleTools.UserEntryIsValidIfNotNullOrWhiteSpace(),
            SetValue = (settings, userEntry) => settings.BirdPiBackupDirectory = userEntry.Trim()
        },
        new SettingsFileProperty<BirdPiBackupSettings>
        {
            PropertyDisplayName = "BirdPi Host",
            PropertyEntryHelp =
                "The host value for the BirdPi.",
            HideEnteredValue = false,
            PropertyIsValid = ObfuscatedSettingsConsoleTools.PropertyIsValidIfNotNullOrWhiteSpace<BirdPiBackupSettings>(x => x.BirdPiHost),
            UserEntryIsValid = ObfuscatedSettingsConsoleTools.UserEntryIsValidIfNotNullOrWhiteSpace(),
            SetValue = (settings, userEntry) => settings.BirdPiHost = userEntry.Trim()
        },
        new SettingsFileProperty<BirdPiBackupSettings>
        {
            PropertyDisplayName = "Remote Home Directory containing the BirdNET-Pi Directory",
            PropertyEntryHelp =
                "The parent, home, directory containing the BirdNET-Pi directory - this may be the home directory of the user used to login via SFTP, but not always...",
            HideEnteredValue = false,
            PropertyIsValid = ObfuscatedSettingsConsoleTools.PropertyIsValidIfNotNullOrWhiteSpace<BirdPiBackupSettings>(x => x.BirdPiRemoteHomeDirectory),
            UserEntryIsValid = ObfuscatedSettingsConsoleTools.UserEntryIsValidIfNotNullOrWhiteSpace(),
            SetValue = (settings, userEntry) => settings.BirdPiRemoteHomeDirectory = userEntry.Trim()
        },
    ]
};

var settingsSetupResult = await settingFileReadAndSetup.Setup();

if (!settingsSetupResult.isValid)
{
    return;
}

var settings = settingsSetupResult.settings;

var backupParentDirectory = new DirectoryInfo(settings.BirdPiBackupDirectory);

if (!backupParentDirectory.Exists)
{
    backupParentDirectory.Create();
}

var dateVersionedBackupDirectory =
    backupParentDirectory.CreateSubdirectory($"BirdPiBackup-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}");

Log.Verbose("Writing Backups to {backupDirectory}", dateVersionedBackupDirectory.FullName);

var sftpClient = new SftpClient(settings.BirdPiHost, settings.BirdPiSftpUser, settings.BirdPiSftpPassword);
sftpClient.Connect();

var birdDbLocalBackupFile = Path.Combine(dateVersionedBackupDirectory.FullName, "birds.db");
var birdDbRemotePath = Path.Combine(settings.BirdPiRemoteHomeDirectory, "BirdNET-Pi/scripts/birds.db");
Log.Verbose("Writing {remotePath} to {localPath}", birdDbRemotePath, birdDbLocalBackupFile);
await SftpTools.DownloadFile(sftpClient, birdDbRemotePath,
    birdDbLocalBackupFile);

var birdnetConfLocalBackupFile = Path.Combine(dateVersionedBackupDirectory.FullName, "birdnet.conf");
var birdnetConfRemotePath = Path.Combine(settings.BirdPiRemoteHomeDirectory, "BirdNET-Pi/birdnet.conf");
Log.Verbose("Writing {remotePath} to {localPath}", birdnetConfRemotePath, birdnetConfLocalBackupFile);
await SftpTools.DownloadFile(sftpClient, birdnetConfRemotePath,
    birdnetConfLocalBackupFile);

var appriseTxtLocalBackupFile = Path.Combine(dateVersionedBackupDirectory.FullName, "apprise.txt");
var appriseTxtRemotePath = Path.Combine(settings.BirdPiRemoteHomeDirectory, "BirdNET-Pi/apprise.txt");
Log.Verbose("Writing {remotePath} to {localPath}", appriseTxtRemotePath, appriseTxtLocalBackupFile);
await SftpTools.DownloadFile(sftpClient, appriseTxtRemotePath,
    appriseTxtLocalBackupFile);

var commonBackupDirectory =
    new DirectoryInfo(Path.Combine(backupParentDirectory.FullName, "BirdPiBackupCommon"));

if (!commonBackupDirectory.Exists) commonBackupDirectory.Create();

await SftpTools.DownloadDirectoriesAndRegularFiles(sftpClient,
    Path.Combine(settings.BirdPiRemoteHomeDirectory, "BirdSongs/Extracted/By_Date"),
    Path.Combine(commonBackupDirectory.FullName, "Extracted", "By_Date"), new ConsoleProgress());
await SftpTools.DownloadDirectoriesAndRegularFiles(sftpClient,
    Path.Combine(settings.BirdPiRemoteHomeDirectory, "BirdSongs/Extracted/Charts"),
    Path.Combine(commonBackupDirectory.FullName, "Extracted", "Charts"), new ConsoleProgress());

Log.Information("Finished {jobName}", "BirdPiBackup");

