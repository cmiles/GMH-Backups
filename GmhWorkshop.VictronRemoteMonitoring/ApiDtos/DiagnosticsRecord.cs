namespace GmhWorkshop.VictronRemoteMonitoring.ApiDtos;

public class DiagnosticsRecord
{
    public int idSite { get; set; }
    public int? timestamp { get; set; }
    public string Device { get; set; }
    public int instance { get; set; }
    public int idDataAttribute { get; set; }
    public string description { get; set; }
    public string formatWithUnit { get; set; }
    public string dbusServiceType { get; set; }
    public string dbusPath { get; set; }
    public string code { get; set; }
    public int bitmask { get; set; }
    public object formattedValue { get; set; }
    public object rawValue { get; set; }
    public DiagnosticsDataAttributeEnumValue[] dataAttributeEnumValues { get; set; }
    public int id { get; set; }
}