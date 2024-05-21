using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GmhWorkshop.CommonTools;

namespace GmhWorkshop.TempestWeatherBackup;
public class TempestWeatherBackupSettings : ISettingsFileType
{
    public const string SettingsTypeIdentifier = nameof(TempestWeatherBackupSettings);
    public string SettingsType { get; set; } = SettingsTypeIdentifier;
    public string TempestAccessToken { get; set; }
    public int TempestDeviceId { get; set; }
    public string TempestFileBackupDirectory { get; set; }
    public int TempestDaysBack { get; set; }
}
