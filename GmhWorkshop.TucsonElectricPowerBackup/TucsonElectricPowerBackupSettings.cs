using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GmhWorkshop.CommonTools;

namespace GmhWorkshop.TucsonElectricPowerBackup;
public class TucsonElectricPowerBackupSettings : ISettingsFileType
{
    public const string SettingsTypeIdentifier = nameof(TucsonElectricPowerBackupSettings);
    public string SettingsType { get; set; } = SettingsTypeIdentifier;
    public string TepEmail { get; set; }
    public string TepPassword { get; set; }
    public string TepBackupDirectory { get; set; }
}
