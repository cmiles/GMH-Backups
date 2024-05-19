using Renci.SshNet;
using Renci.SshNet.Sftp;
using Serilog;
using System.Text.RegularExpressions;

namespace GmhWorkshop.CommonTools;

public static class Sftp
{
    public static void DirectoryList(
        SftpClient sftpClient, ISftpFile sourceRemoteDirectory, List<ISftpFile> directoryList, IProgress<string> progress)
    {
        if (!sftpClient.IsConnected)
        {
            sftpClient.Connect();
        }

        var subDirectories = sftpClient.ListDirectory(sourceRemoteDirectory.FullName).Where(x => x.IsDirectory && x.Name is not ("." or ".."))
            .OrderBy(x => x.FullName).ToList();
            
        directoryList.AddRange(subDirectories);

        if(subDirectories.Count > 0) progress.Report($" {sourceRemoteDirectory.FullName}: +{subDirectories.Count} Directories");

        foreach (var directory in subDirectories)
        {
            DirectoryList(sftpClient, directory, directoryList, progress);
        }

        return;
    }

    /// <summary>
    ///     Downloads the directory structure and REGULAR FILES via SFTP - symlinks, ports, pipes, etc... are all ignored. This method
    /// should be relevant for some 'data backups' but beware that it is not designed to produce a mirror of the source!!!
    /// </summary>
    /// <param name="sftpClient"></param>
    /// <param name="sourceRemotePath"></param>
    /// <param name="destinationLocalPath"></param>
    /// <param name="progress"></param>
    /// <param name="replaceIllegalFileCharacters"></param>
    /// <returns></returns>
    public static async Task DownloadDirectoriesAndRegularFiles(
        SftpClient sftpClient, string sourceRemotePath, string destinationLocalPath, IProgress<string> progress,
        bool replaceIllegalFileCharacters = true)
    {
        //The link below has more elegant code but I like how the progress messages lay out in this version.
        //https://stackoverflow.com/questions/52392766/downloading-a-directory-using-ssh-net-sftp-in-c-sharp

        if (!sftpClient.IsConnected)
        {
            sftpClient.Connect();
        }

        if (!Directory.Exists(destinationLocalPath))
        {
            Directory.CreateDirectory(destinationLocalPath);
        }

        var rootDirectory = sftpClient.Get(sourceRemotePath);

        var allDirectoriesList = new List<ISftpFile> { rootDirectory };

        progress.Report("Finding All Directories...");

        DirectoryList(sftpClient, rootDirectory, allDirectoriesList, progress);

        allDirectoriesList = allDirectoriesList.OrderBy(x => x.FullName).ToList();

        Log.Verbose("Found {directoryCount} directories starting from {remotePath}", allDirectoriesList.Count,
            sourceRemotePath);

        var directoryDownloadProgressCount = 0;

        foreach (var loopDirectory in allDirectoriesList)
        {
            directoryDownloadProgressCount++;

            //Create the directory regardless of whether there are any files to download
            var localDirectory = new DirectoryInfo(Path.Combine(destinationLocalPath, Regex.Replace(loopDirectory.FullName.Replace(rootDirectory.FullName, ""),  @"\A[/\\]*", "")));
                
            if (!localDirectory.Exists)
            {
                localDirectory.Create();

            }

            var filesToDownload = sftpClient.ListDirectory(loopDirectory.FullName).Where(x => x.IsRegularFile && x.Name is not ("." or "..")).OrderBy(x => x.FullName)
                .ToList();

            Log.Verbose("SFTP Directory Download - {loopDirectory} - {filesCount} files - {directoryDownloadProgressCount} of {allDirectoriesCount} Directories", loopDirectory.FullName, filesToDownload.Count, directoryDownloadProgressCount, allDirectoriesList.Count);

            var fileCount = 0;
            var incrementCount = filesToDownload.Count / 32 == 0 ? filesToDownload.Count == 0 ? 1 : filesToDownload.Count : filesToDownload.Count / 10;

            foreach (var loopFiles in filesToDownload)
            {
                fileCount++;

                var destinationFilePath = Path.Combine(localDirectory.FullName, 
                    replaceIllegalFileCharacters ? loopFiles.Name.Replace(":", "[colon]") : loopFiles.Name);

                if (fileCount % incrementCount == 0)
                {
                    progress.Report($"   SFTP File Download - {fileCount} of {filesToDownload.Count}");
                    progress.Report($"      to: {destinationFilePath}");
                }

                //TODO: MD5 Checks?
                if (File.Exists(destinationFilePath))
                {
                    continue;
                }

                await DownloadFile(sftpClient, loopFiles.FullName, destinationFilePath);
                await using Stream fileStream = File.Create(destinationFilePath);
                sftpClient.DownloadFile(loopFiles.FullName, fileStream);
            }
        }
    }

    /// <summary>
    ///     Downloads a file via SFTP - the client will be connected if not already connected (and left connected)
    /// </summary>
    /// <param name="sftpClient"></param>
    /// <param name="sourceRemotePath"></param>
    /// <param name="destinationLocalPath"></param>
    /// <returns></returns>
    public static async Task DownloadFile(
        SftpClient sftpClient, string sourceRemotePath, string destinationLocalPath)
    {
        if (!sftpClient.IsConnected)
        {
            sftpClient.Connect();
        }

        await using Stream dbBackupFileStream = File.Create(destinationLocalPath);
        sftpClient.DownloadFile(sourceRemotePath, dbBackupFileStream);
    }
}