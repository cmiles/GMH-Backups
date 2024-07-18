using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmhWorkshop.VictronRemoteMonitoring.ApiDtos;

public class DiagnosticsResponse
{
    public bool success { get; set; }
    public DiagnosticsRecord[] records { get; set; }
    public int num_records { get; set; }
}