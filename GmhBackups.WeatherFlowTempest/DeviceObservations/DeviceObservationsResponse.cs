namespace GmhBackups.WeatherFlowTempest.DeviceObservations;

public class DeviceObservationsResponse
{
    public int? bucket_step_minutes { get; set; }
    public int? device_id { get; set; }
    public decimal?[][]? obs { get; set; }
    public string? source { get; set; }
    public Status? status { get; set; }
    public string? type { get; set; }
}