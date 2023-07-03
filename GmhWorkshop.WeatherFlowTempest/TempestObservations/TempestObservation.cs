namespace GmhWorkshop.WeatherFlowTempest.TempestObservations
{
    /// <summary>
    /// A WeatherFlow Tempest Observation with light translation into typed fields
    /// </summary>
    public class TempestObservation
    {
        //2023-06-27 - Retrieved from https://weatherflow.github.io/Tempest/api/swagger/
        //The numbers are the order of the fields in the obs array objects and 
        //these numbers are specific to the Tempest (there are two other kinds
        //of devices - each has its own array definition.
        //0 - Epoch(Seconds UTC)
        //1 - Wind Lull(m/s)
        //2 - Wind Avg(m/s)
        //3 - Wind Gust(m/s)
        //4 - Wind Direction(degrees)
        //5 - Wind Sample Interval(seconds)
        //6 - Pressure(MB)
        //7 - Air Temperature(C)
        //8 - Relative Humidity(%)
        //9 - Illuminance(lux)
        //10 - UV(index)
        //11 - Solar Radiation(W/m^2)
        //12 - Rain Accumulation(mm)
        //13 - Precipitation Type(0 = none, 1 = rain, 2 = hail, 3 = rain + hail (experimental))
        //14 - Average Strike Distance(km)
        //15 - Strike Count
        //16 - Battery(volts)
        //17 - Report Interval(minutes)
        //18 - Local Day Rain Accumulation(mm)
        //19 - NC Rain Accumulation(mm)
        //20 - Local Day NC Rain Accumulation(mm)
        //21 - Precipitation Analysis Type(0 = none, 1 = Rain Check with user display on, 2 = Rain Check with user display off)
        //
        //https://help.weatherflow.com/hc/en-us/articles/360024436634
        //NC Rain is an additional data source provided via the Tempest Weather System, powered by Nearcast
        //Technology. NC Rain data is an estimate of the average rainfall accumulation around your Tempest
        //at the neighborhood level (roughly 100m x 100m). The NC Rain value is derived using measurements
        //from your Tempest device as well as other relevant measures of precipitation such as radar, nearby
        //professional rain gauges and other Tempest devices. NC Rain is our most valuable and useful
        //indicator of rain accumulation for a given location.

        /// <summary>
        ///     7 - Air Temperature(C)
        /// </summary>
        public decimal? AirTemperature { get; set; }

        /// <summary>
        ///     14 - Average Strike Distance(km)
        /// </summary>
        public decimal? AverageLightningStrikeDistance { get; set; }

        /// <summary>
        ///     16 - Battery(volts)
        /// </summary>
        public decimal? Battery { get; set; }

        /// <summary>
        ///     9 - Illuminance(lux)
        /// </summary>
        public decimal? Illuminance { get; set; }

        /// <summary>
        ///     15 - Strike Count
        /// </summary>
        public decimal? LightningStrikeCount { get; set; }

        /// <summary>
        ///     20 - Local Day NC Rain Accumulation(mm). NC Rain data is an estimate of the average rainfall accumulation around
        ///     your Tempest
        ///     at the neighborhood level (roughly 100m x 100m)
        /// </summary>
        public decimal? LocalDayNearCastRainAccumulation { get; set; }

        /// <summary>
        ///     18 - Local Day Rain Accumulation(mm)
        /// </summary>
        public decimal? LocalDayRainAccumulation { get; set; }

        /// <summary>
        ///     19 - NC Rain Accumulation(mm). NC Rain data is an estimate of the average rainfall accumulation around your Tempest
        ///     at the neighborhood level (roughly 100m x 100m)
        /// </summary>
        public decimal? NearCastRainAccumulation { get; set; }

        /// <summary>
        ///     21 - Precipitation Analysis Type(0 = none, 1 = Rain Check with user display on, 2 = Rain Check with user display
        ///     off)
        /// </summary>
        public int? PrecipitationAnalysisType { get; set; }

        /// <summary>
        ///     String Translation of Precipitation Analysis Type(0 = none, 1 = Rain Check with user display on, 2 = Rain Check
        ///     with user display off)
        /// </summary>
        public string PrecipitationAnalysisTypeTranslation { get; set; } = string.Empty;

        /// <summary>
        ///     13 - Precipitation Type(0 = none, 1 = rain, 2 = hail, 3 = rain + hail (experimental))
        /// </summary>
        public int? PrecipitationType { get; set; }

        /// <summary>
        ///     String Translation of Precipitation Type(0 = none, 1 = rain, 2 = hail, 3 = rain + hail (experimental))
        /// </summary>
        public string PrecipitationTypeTranslated { get; set; } = string.Empty;

        /// <summary>
        ///     6 - Pressure(MB)
        /// </summary>
        public decimal? Pressure { get; set; }

        /// <summary>
        ///     12 - Rain Accumulation(mm)
        /// </summary>
        public decimal? RainAccumulation { get; set; }

        /// <summary>
        ///     0 - Epoch(Seconds UTC) -> Recorded On
        /// </summary>
        public DateTime RecordedOnUtc { get; set; }

        public long RecordedOnEpoch { get; set; }

        /// <summary>
        ///     8 - Relative Humidity(%)
        /// </summary>
        public decimal? RelativeHumidity { get; set; }

        /// <summary>
        ///     17 - Report Interval(minutes)
        /// </summary>
        public decimal? ReportInterval { get; set; }

        /// <summary>
        ///     11 - Solar Radiation(W/m^2)
        /// </summary>
        public decimal? SolarRadiation { get; set; }

        /// <summary>
        ///     10 - UV(index)
        /// </summary>
        public decimal? Uv { get; set; }

        /// <summary>
        ///     2 - Wind Avg(m/s)
        /// </summary>
        public decimal? WindAvg { get; set; }

        /// <summary>
        ///     4 - Wind Direction(degrees)
        /// </summary>
        public decimal? WindDirection { get; set; }

        /// <summary>
        ///     3 - Wind Gust(m/s)
        /// </summary>
        public decimal? WindGust { get; set; }

        /// <summary>
        ///     1 - Wind Lull(m/s)
        /// </summary>
        public decimal? WindLull { get; set; }

        /// <summary>
        ///     5 - Wind Sample Interval(seconds)
        /// </summary>
        public decimal? WindSampleInterval { get; set; }

        public static TempestObservation CreateInstance(decimal?[] observation)
        {
            var toReturn = new TempestObservation();
            toReturn.AirTemperature = observation[7];
            toReturn.AverageLightningStrikeDistance = observation[14];
            toReturn.Battery = observation[16];
            toReturn.Illuminance = observation[9];
            toReturn.LightningStrikeCount = observation[15];
            toReturn.LocalDayNearCastRainAccumulation = observation[20];
            toReturn.LocalDayRainAccumulation = observation[18];
            toReturn.NearCastRainAccumulation = observation[19];
            toReturn.PrecipitationAnalysisType = (int?)observation[21];
            toReturn.PrecipitationAnalysisTypeTranslation = toReturn.PrecipitationAnalysisType == null ? string.Empty : 
                TempestObservationLookups.TempestPrecipitationAnalysisType[toReturn.PrecipitationAnalysisType.Value];
            toReturn.PrecipitationType = (int?)observation[13];
            toReturn.PrecipitationTypeTranslated = toReturn.PrecipitationType == null ? string.Empty :
                TempestObservationLookups.TempestPrecipitationType[toReturn.PrecipitationType.Value];
            toReturn.Pressure = observation[6];
            toReturn.RainAccumulation = observation[12];
            toReturn.RecordedOnEpoch = (long)observation[0]!;
            toReturn.RecordedOnUtc = DateTimeOffset.FromUnixTimeSeconds((long)observation[0]!).UtcDateTime;
            toReturn.RelativeHumidity = observation[8];
            toReturn.ReportInterval = observation[17];
            toReturn.SolarRadiation = observation[11];
            toReturn.Uv = observation[10];
            toReturn.WindAvg = observation[2];
            toReturn.WindDirection = observation[4];
            toReturn.WindGust = observation[3];
            toReturn.WindLull = observation[1];
            toReturn.WindSampleInterval = observation[5];

            return toReturn;
        }
    }
}