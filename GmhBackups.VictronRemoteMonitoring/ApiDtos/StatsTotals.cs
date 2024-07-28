namespace GmhBackups.VictronRemoteMonitoring.ApiDtos;

public class StatsTotals
{
    public int Instance { get; set; }
    public Dictionary<string, decimal?> Totals { get; set; }
}