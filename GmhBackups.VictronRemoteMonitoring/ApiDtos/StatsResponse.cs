using GmhBackups.VictronRemoteMonitoring.Models;

namespace GmhBackups.VictronRemoteMonitoring.ApiDtos;

public class StatsResponse
{
    public bool Success { get; set; }
    public required List<StatsRecord> Records { get; set; }
    public required List<StatsTotals> Totals { get; set; }

    public List<VrmStat> ToVrmStats(List<DiagnosticsRecord> diagnostics)
    {
        var returnList = new List<VrmStat>();

        foreach (var record in Records)
        {
            foreach (var loopStatCodes in record.Stats)
            {
                decimal? total = null;
                if (Totals.Any(x => x.Instance == record.Instance))
                {
                    if (Totals.First(x => x.Instance == record.Instance).Totals
                        .TryGetValue(loopStatCodes.Key, out var possibleTotal))
                    {
                        total = possibleTotal;
                    }
                }

                var possibleDiagnostic = diagnostics.FirstOrDefault(x => x.instance == record.Instance && x.code == loopStatCodes.Key) ?? diagnostics.FirstOrDefault(x => x.code == loopStatCodes.Key);

                var toAdd = new VrmStat
                {
                    StatCode = loopStatCodes.Key,
                    Records = loopStatCodes.Value?.ToList(),
                    Total = total,
                    DiagnosticInformation = possibleDiagnostic,
                    Instance = record.Instance
                };

                returnList.Add(toAdd);
            }
        }

        return returnList;
    }
}