namespace GmhBackups.VictronRemoteMonitoring.ApiDtos;

public class StatsRecord
{
    public int Instance { get; set; }
    public Dictionary<string, decimal[][]?> Stats { get; set; }
}