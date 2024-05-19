namespace GmhWorkshop.VictronRemoteMonitoring.Models;

public abstract class InstallationsResponse
{
    public bool success { get; set; }
    public InstallationRecord[] records { get; set; }
}