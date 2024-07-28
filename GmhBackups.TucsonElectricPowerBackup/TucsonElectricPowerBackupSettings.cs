using PointlessWaymarks.VaultfuscationTools;

namespace GmhBackups.TucsonElectricPowerBackup;

public class TucsonElectricPowerBackupSettings : ISettingsFileType
{
    public const string SettingsTypeIdentifier = nameof(TucsonElectricPowerBackupSettings);
    public string TepEmail { get; set; } = string.Empty;
    public string TepPassword { get; set; } = string.Empty;
    public string TepBackupDirectory { get; set; } = string.Empty;
    public string SettingsType { get; set; } = SettingsTypeIdentifier;
}