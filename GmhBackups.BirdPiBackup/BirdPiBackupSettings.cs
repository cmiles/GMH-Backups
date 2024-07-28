using PointlessWaymarks.VaultfuscationTools;

namespace GmhBackups.BirdPiBackup;

public class BirdPiBackupSettings : ISettingsFileType
{
    public const string SettingsTypeIdentifier = nameof(BirdPiBackupSettings);
    public string BirdPiBackupDirectory { get; set; } = string.Empty;
    public string BirdPiHost { get; set; } = string.Empty;
    public string BirdPiRemoteHomeDirectory { get; set; } = string.Empty;
    public string BirdPiSftpPassword { get; set; } = string.Empty;
    public string BirdPiSftpUser { get; set; } = string.Empty;
    public string SettingsType { get; set; } = SettingsTypeIdentifier;
}