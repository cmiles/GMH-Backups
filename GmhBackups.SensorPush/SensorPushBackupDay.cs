namespace GmhBackups.SensorPush;

public class SensorPushBackupDay
{
    public DateOnly Date { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public List<Sample> Samples { get; set; } = new();
}