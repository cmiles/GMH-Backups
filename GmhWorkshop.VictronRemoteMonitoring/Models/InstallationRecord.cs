namespace GmhWorkshop.VictronRemoteMonitoring.Models;

public abstract class InstallationRecord
{
    public int idSite { get; set; }
    public int accessLevel { get; set; }
    public bool owner { get; set; }
    public bool is_admin { get; set; }
    public string name { get; set; }
    public string identifier { get; set; }
    public int idUser { get; set; }
    public int pvMax { get; set; }
    public string timezone { get; set; }
    public object phonenumber { get; set; }
    public object notes { get; set; }
    public object geofence { get; set; }
    public bool geofenceEnabled { get; set; }
    public bool realtimeUpdates { get; set; }
    public int hasMains { get; set; }
    public int hasGenerator { get; set; }
    public object noDataAlarmTimeout { get; set; }
    public int alarmMonitoring { get; set; }
    public int invalidVRMAuthTokenUsedInLogRequest { get; set; }
    public int syscreated { get; set; }
    public int grafanaEnabled { get; set; }
    public int isPaygo { get; set; }
    public object paygoCurrency { get; set; }
    public object paygoTotalAmount { get; set; }
    public int inverterChargerControl { get; set; }
    public bool shared { get; set; }
    public string device_icon { get; set; }
}