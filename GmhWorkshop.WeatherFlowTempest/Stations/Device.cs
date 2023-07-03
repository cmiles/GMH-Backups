public class Device
{
    public long device_id { get; set; }
    public Device_Meta device_meta { get; set; }
    public string device_type { get; set; }
    public string firmware_revision { get; set; }
    public string hardware_revision { get; set; }
    public long location_id { get; set; }
    public string serial_number { get; set; }
    public Device_Settings device_settings { get; set; }
}