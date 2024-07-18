namespace GmhWorkshop.VictronRemoteMonitoring.ApiDtos;

public class InstallationsResponse
{
    public bool success { get; set; }
    public InstallationRecord[] records { get; set; }
}