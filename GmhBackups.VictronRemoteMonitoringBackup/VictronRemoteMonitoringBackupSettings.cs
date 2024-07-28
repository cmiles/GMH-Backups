using PointlessWaymarks.VaultfuscationTools;

namespace GmhBackups.VictronRemoteMonitoringBackup;

public class VictronRemoteMonitoringBackupSettings : ISettingsFileType
{
    public const string SettingsTypeIdentifier = nameof(VictronRemoteMonitoringBackupSettings);
    public string VrmEmail { get; set; } = string.Empty;
    public string VrmPassword { get; set; } = string.Empty;
    public string VrmBackupDirectory { get; set; } = string.Empty;
    public int VrmDaysBack { get; set; }
    public string SettingsType { get; set; } = SettingsTypeIdentifier;
}