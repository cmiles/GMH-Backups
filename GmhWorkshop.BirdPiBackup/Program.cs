using GmhWorkshop.BirdPiBackup;
using Serilog;
using Serilog.Enrichers.CallerInfo;
using Renci.SshNet;
using PointlessWaymarks.CommonTools;
using PointlessWaymarks.VaultfuscationTools;
using Serilog.Extensions.Logging;
using Microsoft.Extensions.Logging;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .Enrich.WithCallerInfo(true,
        "GmhWorkshop.",
        "gmhworkshop")
    .Enrich
    .FromGlobalLogContext()
    .MinimumLevel.Verbose()
    .LogToConsole()
    .CreateLogger();

AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
{
    Log.Fatal(eventArgs.ExceptionObject as Exception,
        $"Unhandled Exception {(eventArgs.ExceptionObject as Exception)?.Message ?? ""}");
    Log.CloseAndFlush();
};

if (args.Length < 1)
{
    Console.WriteLine("You must provide the name for the settings file to use - this can be an existing settings file or a new file to create.");

    Log.Error(
        $"The Settings File must be provided as the only argument to this program (found {args.Length} arguments)");
    await Log.CloseAndFlushAsync();
    return;
}

var cleanedSettingsFile = args[0].Trim();

var interactive = !args.Any(x => x.Contains("-notinteractive", StringComparison.OrdinalIgnoreCase));
var promptAsIfNewFile = args.Any(x => x.Contains("-redo", StringComparison.OrdinalIgnoreCase));

var msLogger = new SerilogLoggerFactory(Log.Logger)
    .CreateLogger<ObfuscatedSettingsConsoleSetup<BirdPiBackupSettings>>();

var settingFileReadAndSetup = new ObfuscatedSettingsConsoleSetup<BirdPiBackupSettings>(msLogger)
{
    SettingsFile = cleanedSettingsFile,
    SettingsFileIdentifier = BirdPiBackupSettings.SettingsTypeIdentifier,
    VaultServiceIdentifier = "http://birdpibackup.private",
    Interactive = interactive,
    PromptAsIfNewFile = promptAsIfNewFile,
    SettingsFileProperties =
    [
        new SettingsFileProperty<BirdPiBackupSettings>
        {
            PropertyDisplayName = "SFTP User Name",
            PropertyEntryHelp =
                "The User Name used to login via SFTP to the BirdPi.",
            HideEnteredValue = false,
            PropertyIsValid = ObfuscatedSettingsHelpers.PropertyIsValidIfNotNullOrWhiteSpace<BirdPiBackupSettings>(x => x.BirdPiSftpUser),
            UserEntryIsValid = ObfuscatedSettingsHelpers.UserEntryIsValidIfNotNullOrWhiteSpace(),
            SetValue = (settings, userEntry) => settings.BirdPiSftpUser = userEntry.Trim()
        },
        new SettingsFileProperty<BirdPiBackupSettings>
        {
            PropertyDisplayName = "SFTP Password",
            PropertyEntryHelp =
                "The SFTP password for the BirdPi.",
            HideEnteredValue = true,
            PropertyIsValid = ObfuscatedSettingsHelpers.PropertyIsValidIfNotNullOrWhiteSpace<BirdPiBackupSettings>(x => x.BirdPiSftpPassword),
            UserEntryIsValid = ObfuscatedSettingsHelpers.UserEntryIsValidIfNotNullOrWhiteSpace(),
            SetValue = (settings, userEntry) => settings.BirdPiSftpPassword = userEntry.Trim()
        },
        new SettingsFileProperty<BirdPiBackupSettings>
        {
            PropertyDisplayName = "Local Backup Directory",
            PropertyEntryHelp =
                "The backup directory will be used to save the backup data - it is best to dedicate a directory just to this data to avoid conflicts with other data.",
            HideEnteredValue = false,
            PropertyIsValid =
                ObfuscatedSettingsHelpers.PropertyIsValidIfNotNullOrWhiteSpace<BirdPiBackupSettings>(x => x.BirdPiBackupDirectory),
            UserEntryIsValid = ObfuscatedSettingsHelpers.UserEntryIsValidIfNotNullOrWhiteSpace(),
            SetValue = (settings, userEntry) => settings.BirdPiBackupDirectory = userEntry.Trim()
        },
        new SettingsFileProperty<BirdPiBackupSettings>
        {
            PropertyDisplayName = "BirdPi Host",
            PropertyEntryHelp =
                "The host value for the BirdPi.",
            HideEnteredValue = false,
            PropertyIsValid = ObfuscatedSettingsHelpers.PropertyIsValidIfNotNullOrWhiteSpace<BirdPiBackupSettings>(x => x.BirdPiHost),
            UserEntryIsValid = ObfuscatedSettingsHelpers.UserEntryIsValidIfNotNullOrWhiteSpace(),
            SetValue = (settings, userEntry) => settings.BirdPiHost = userEntry.Trim()
        },
        new SettingsFileProperty<BirdPiBackupSettings>
        {
            PropertyDisplayName = "Remote Home Directory containing the BirdNET-Pi Directory",
            PropertyEntryHelp =
                "The parent, home, directory containing the BirdNET-Pi directory - this may be the home directory of the user used to login via SFTP, but not always...",
            HideEnteredValue = false,
            PropertyIsValid = ObfuscatedSettingsHelpers.PropertyIsValidIfNotNullOrWhiteSpace<BirdPiBackupSettings>(x => x.BirdPiRemoteHomeDirectory),
            UserEntryIsValid = ObfuscatedSettingsHelpers.UserEntryIsValidIfNotNullOrWhiteSpace(),
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
var birdDbRemotePath = StringTools.UrlCombine(settings.BirdPiRemoteHomeDirectory, "BirdNET-Pi/scripts/birds.db");
Log.Verbose("Writing {remotePath} to {localPath}", birdDbRemotePath, birdDbLocalBackupFile);
await SftpHelpers.DownloadFile(sftpClient, birdDbRemotePath,
    birdDbLocalBackupFile);

var birdnetConfLocalBackupFile = Path.Combine(dateVersionedBackupDirectory.FullName, "birdnet.conf");
var birdnetConfRemotePath = StringTools.UrlCombine(settings.BirdPiRemoteHomeDirectory, "BirdNET-Pi/birdnet.conf");
Log.Verbose("Writing {remotePath} to {localPath}", birdnetConfRemotePath, birdnetConfLocalBackupFile);
await SftpHelpers.DownloadFile(sftpClient, birdnetConfRemotePath,
    birdnetConfLocalBackupFile);

var appriseTxtLocalBackupFile = Path.Combine(dateVersionedBackupDirectory.FullName, "apprise.txt");
var appriseTxtRemotePath = StringTools.UrlCombine(settings.BirdPiRemoteHomeDirectory, "BirdNET-Pi/apprise.txt");
Log.Verbose("Writing {remotePath} to {localPath}", appriseTxtRemotePath, appriseTxtLocalBackupFile);
await SftpHelpers.DownloadFile(sftpClient, appriseTxtRemotePath,
    appriseTxtLocalBackupFile);

var commonBackupDirectory =
    new DirectoryInfo(Path.Combine(backupParentDirectory.FullName, "BirdPiBackupCommon"));

if (!commonBackupDirectory.Exists) commonBackupDirectory.Create();

await SftpHelpers.DownloadDirectoriesAndRegularFiles(sftpClient,
    StringTools.UrlCombine(settings.BirdPiRemoteHomeDirectory, "BirdSongs/Extracted/By_Date"),
    Path.Combine(commonBackupDirectory.FullName, "Extracted", "By_Date"), new ConsoleProgress());
await SftpHelpers.DownloadDirectoriesAndRegularFiles(sftpClient,
    StringTools.UrlCombine(settings.BirdPiRemoteHomeDirectory, "BirdSongs/Extracted/Charts"),
    Path.Combine(commonBackupDirectory.FullName, "Extracted", "Charts"), new ConsoleProgress());

Log.Information("Finished {jobName}", "BirdPiBackup");

