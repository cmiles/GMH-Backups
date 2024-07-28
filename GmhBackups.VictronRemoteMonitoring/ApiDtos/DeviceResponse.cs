namespace GmhBackups.VictronRemoteMonitoring.ApiDtos;

public class DeviceResponse
{
    public bool success { get; set; }
    public DeviceRecords records { get; set; }
}