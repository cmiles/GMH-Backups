using System.Security.Cryptography.X509Certificates;

namespace GmhWorkshop.WeatherFlowTempest.TempestObservations
{
    /// <summary>
    /// Lookups for some coded Tempest Observation Fields
    /// </summary>
    public static class TempestObservationLookups
    {
        public static Dictionary<int, string> TempestPrecipitationAnalysisType => new Dictionary<int, string>
            { { 0, "None" }, { 1, "Rain Check With User Display On" }, { 2, "Rain Check With User Display Off" } };

        public static Dictionary<int, string> TempestPrecipitationType => new Dictionary<int, string>
            { { 0, "None" }, { 1, "Rain" }, { 2, "Hail" }, {3, "Rain and Hail"} };
    }
}