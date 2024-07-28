using PointlessWaymarks.VaultfuscationTools;

namespace GmhBackups.TempestWeatherBackup;

public class TempestWeatherBackupSettings : ISettingsFileType
{
    public const string SettingsTypeIdentifier = nameof(TempestWeatherBackupSettings);
    public string TempestAccessToken { get; set; } = string.Empty;
    public int TempestDeviceId { get; set; }
    public string TempestFileBackupDirectory { get; set; } = string.Empty;
    public int TempestDaysBack { get; set; }
    public string SettingsType { get; set; } = SettingsTypeIdentifier;
}