using GmhWorkshop.CommonTools;
using Renci.SshNet;
using Serilog;

namespace GmhWorkshop.Jobs;

public static class BirdPiBackup
{
    public static async Task Run(WorkshopSettings settings, IProgress<string> progress)
    {
        Log.Information("Starting {jobName}", "BirdPiBackup");

        if (string.IsNullOrWhiteSpace(settings.BirdPiHost))
        {
            Log.Error("BirdPiHost is Blank");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.BirdPiBackupDirectory))
        {
            Log.Error("BirdPiBackupDirectory is Blank");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.BirdPiSftpUser))
        {
            Log.Error("BirdPiSftpUser is Blank");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.BirdPiSftpPassword))
        {
            Log.Error("BirdPiSftpPassword is Blank");
            return;
        }

        var backupParentDirectory = new DirectoryInfo(settings.BirdPiBackupDirectory);

        if (!backupParentDirectory.Exists)
        {
            backupParentDirectory.Create();
        }

        var backupDirectory =
            backupParentDirectory.CreateSubdirectory($"BirdPiBackup-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}");

        Log.Verbose("Writing Backups to {backupDirectory}", backupDirectory.FullName);

        var sftpClient = new SftpClient(settings.BirdPiHost, settings.BirdPiSftpUser, settings.BirdPiSftpPassword);
        sftpClient.Connect();

        var birdDbLocalBackupFile = Path.Combine(backupDirectory.FullName, "birds.db");
        var birdDbRemotePath = Path.Combine(settings.BirdPiRemoteHomeDirectory, "BirdNET-Pi/scripts/birds.db");
        Log.Verbose("Writing {remotePath} to {localPath}", birdDbRemotePath, birdDbLocalBackupFile);
        await Sftp.DownloadFile(sftpClient, birdDbRemotePath,
            birdDbLocalBackupFile);

        var birdnetConfLocalBackupFile = Path.Combine(backupDirectory.FullName, "birdnet.conf");
        var birdnetConfRemotePath = Path.Combine(settings.BirdPiRemoteHomeDirectory, "BirdNET-Pi/birdnet.conf");
        Log.Verbose("Writing {remotePath} to {localPath}", birdnetConfRemotePath, birdnetConfLocalBackupFile);
        await Sftp.DownloadFile(sftpClient, birdnetConfRemotePath,
            birdnetConfLocalBackupFile);

        var appriseTxtLocalBackupFile = Path.Combine(backupDirectory.FullName, "apprise.txt");
        var appriseTxtRemotePath = Path.Combine(settings.BirdPiRemoteHomeDirectory, "BirdNET-Pi/apprise.txt");
        Log.Verbose("Writing {remotePath} to {localPath}", appriseTxtRemotePath, appriseTxtLocalBackupFile);
        await Sftp.DownloadFile(sftpClient, appriseTxtRemotePath,
            appriseTxtLocalBackupFile);

        await Sftp.DownloadDirectoriesAndRegularFiles(sftpClient,
            Path.Combine(settings.BirdPiRemoteHomeDirectory, "BirdSongs/Extracted/By_Date"),
            Path.Combine(backupDirectory.FullName, "Extracted", "By_Date"), progress);
        await Sftp.DownloadDirectoriesAndRegularFiles(sftpClient,
            Path.Combine(settings.BirdPiRemoteHomeDirectory, "BirdSongs/Extracted/Charts"),
            Path.Combine(backupDirectory.FullName, "Extracted", "Charts"), progress);

        Log.Information("Finished {jobName}", "BirdPiBackup");
    }
}