namespace GmhWorkshop.VictronRemoteMonitoring;

public abstract class InstallationsResponse
{
    public bool success { get; set; }
    public InstallationRecord[] records { get; set; }
}