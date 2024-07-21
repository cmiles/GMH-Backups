using GmhWorkshop.VictronRemoteMonitoring.ApiDtos;

namespace GmhWorkshop.VictronRemoteMonitoring.Models;

public class VrmInstallationStats
{
    public required InstallationRecord Installation { get; set; }
    public required List<VrmStat> Stats { get; set; }
    public List<Device> Device { get; set; } = [];
}