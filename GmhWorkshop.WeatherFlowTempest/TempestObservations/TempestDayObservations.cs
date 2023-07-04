using Serilog;

namespace GmhWorkshop.WeatherFlowTempest.TempestObservations;

/// <summary>
///     Represents a single UTC day of Tempest weather station observations with basic station
///     information and a list of translated observations. An exception will be thrown if
///     observations are not all on the same date.
/// </summary>
public class TempestDayObservations
{
    public TempestDayObservations(DateOnly utcDate, long deviceId, Station station, List<decimal?[]> observations)
    {
        UtcDate = utcDate;
        DeviceId = deviceId;
        Latitude = station.latitude;
        Longitude = station.longitude;
        Name = station.name;
        StationId = station.station_id;
        Observations = observations.Select(TempestObservation.CreateInstance).Where(x => DateOnly.FromDateTime(x.RecordedOnUtc) == UtcDate).OrderBy(x => x.RecordedOnUtc)
            .ToList();

    }

    public long DeviceId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Name { get; set; }
    public List<TempestObservation> Observations { get; set; }
    public long StationId { get; set; }
    public DateOnly UtcDate { get; set; }
}