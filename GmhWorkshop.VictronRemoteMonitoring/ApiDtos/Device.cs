using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmhWorkshop.VictronRemoteMonitoring.ApiDtos;

public class Device
{
    public string name { get; set; }
    public string customName { get; set; }
    public string productCode { get; set; }
    public int idSite { get; set; }
    public string productName { get; set; }
    public string firmwareVersion { get; set; }
    public string remoteOnLan { get; set; }
    public string autoUpdate { get; set; }
    public string updateTo { get; set; }
    public int lastConnection { get; set; }
    public string _class { get; set; }
    public int loggingInterval { get; set; }
    public string identifier { get; set; }
    public int lastPowerUpOrRestart { get; set; }
    public bool vncSshAuth { get; set; }
    public string vncStatus { get; set; }
    public int vncPort { get; set; }
    public bool twoWayCommunication { get; set; }
    public bool remoteSupportEnabled { get; set; }
    public int remoteSupportPort { get; set; }
    public string remoteSupportIp { get; set; }
    public string remoteSupport { get; set; }
    public string machineSerialNumber { get; set; }
    public object[] settings { get; set; }
    public int instance { get; set; }
    public int idDeviceType { get; set; }
}

