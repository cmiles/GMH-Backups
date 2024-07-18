using PointlessWaymarks.VaultfuscationTools;

namespace GmhWorkshop.SensorPushBackup;

public class SensorPushBackupSettings : ISettingsFileType
{
    public const string SettingsTypeIdentifier = nameof(SensorPushBackupSettings);
    public string SensorPushBackupDirectory { get; set; } = string.Empty;
    public string SensorPushEmail { get; set; } = string.Empty;
    public int SensorPushDaysBack { get; set; }
    public string SensorPushPassword { get; set; } = string.Empty;
    public string SettingsType { get; set; } = SettingsTypeIdentifier;
}