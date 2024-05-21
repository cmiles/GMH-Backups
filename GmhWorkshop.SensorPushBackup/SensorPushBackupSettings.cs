using GmhWorkshop.CommonTools;

namespace GmhWorkshop.SensorPushBackup;

public class SensorPushBackupSettings : ISettingsFileType
{
    public const string SettingsTypeIdentifier = nameof(SensorPushBackupSettings);
    public string SettingsType { get; set; } = SettingsTypeIdentifier;
    public string SensorPushBackupDirectory { get; set; }
    public string SensorPushEmail { get; set; }
    public int SensorPushDaysBack { get; set; }
    public string SensorPushPassword { get; set; }
}