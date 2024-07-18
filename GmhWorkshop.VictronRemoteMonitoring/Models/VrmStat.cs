using GmhWorkshop.VictronRemoteMonitoring.ApiDtos;

namespace GmhWorkshop.VictronRemoteMonitoring.Models;

public class VrmStat
{
    public required string StatCode { get; set; }
    public List<decimal[]>? Records { get; set; } = [];
    public decimal? Total { get; set; }
    public DiagnosticsRecord? DiagnosticInformation { get; set; }
    public int Instance { get; set; }
}