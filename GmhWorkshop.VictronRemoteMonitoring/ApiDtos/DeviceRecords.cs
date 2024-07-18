namespace GmhWorkshop.VictronRemoteMonitoring.ApiDtos;

public class DeviceRecords
{
    public Device[] devices { get; set; }
    public object[] unconfigured_devices { get; set; }
}