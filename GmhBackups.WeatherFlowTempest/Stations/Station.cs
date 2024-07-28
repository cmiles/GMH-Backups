public class Station
{
    public long created_epoch { get; set; }
    public Device[] devices { get; set; }
    public bool is_local_mode { get; set; }
    public long last_modified_epoch { get; set; }
    public decimal latitude { get; set; }
    public long location_id { get; set; }
    public decimal longitude { get; set; }
    public string name { get; set; }
    public string public_name { get; set; }
    public long station_id { get; set; }
    public Station_Items[] station_items { get; set; }
    public Station_Meta station_meta { get; set; }
    public string timezone { get; set; }
    public long timezone_offset_minutes { get; set; }
}