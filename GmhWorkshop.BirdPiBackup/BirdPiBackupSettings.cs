using GmhWorkshop.CommonTools;

namespace GmhWorkshop.BirdPiBackup;

public class BirdPiBackupSettings : ISettingsFileType
{
    public const string SettingsTypeIdentifier = nameof(BirdPiBackupSettings);
    public string SettingsType { get; set; } = SettingsTypeIdentifier;
    public string BirdPiBackupDirectory { get; set; }
    public string BirdPiHost { get; set; }
    public string BirdPiRemoteHomeDirectory { get; set; }
    public string BirdPiSftpPassword { get; set; }
    public string BirdPiSftpUser { get; set; }
}