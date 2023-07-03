using System.Net.Http.Headers;
using System.Text.Json;
using GmhWorkshop.WeatherFlowTempest.DeviceObservations;
using GmhWorkshop.WeatherFlowTempest.TempestObservations;
using Serilog;

namespace GmhWorkshop.WeatherFlowTempest
{
    public static class Observations
    {
        public static async Task<List<(TempestDayObservations observation, string observationsJson)>> GetObservations(
            long deviceId, string accessToken, List<DateOnly> utcDays, IProgress<string> progress)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var returnList = new List<(TempestDayObservations observation, string observationsJson)>();

            var frozenNow = DateOnly.FromDateTime(DateTime.UtcNow);

            var stationResults = await GetStations(accessToken);

            if (stationResults.stationReturn == null || string.IsNullOrWhiteSpace(stationResults.stationsJson))
            {
                Log.Error("WeatherFlow Tempest API did not Find any Stations?");
                return returnList;
            }

            progress.Report($"Found {stationResults.stationsJson.Length} Stations");

            var deviceStationResults =
                stationResults.stationReturn.stations.FirstOrDefault(x => x.devices.Any(y => y.device_id == deviceId));

            if (deviceStationResults == null)
            {
                Log.Error("Did not find a Station for the Device from the WeatherFlow Tempest API?");
                return returnList;
            }

            progress.Report($"Found Device Station - {deviceStationResults.name}");

            var dayProgressCounter = 0;
            var incrementCount = utcDays.Count / 32 == 0 ? utcDays.Count == 0 ? 1 : utcDays.Count : utcDays.Count / 10;

            foreach (var loopDay in utcDays)
            {
                dayProgressCounter++;

                if (dayProgressCounter % incrementCount == 0)
                {
                    progress.Report($"WeatherFlow Day Download {dayProgressCounter} of {utcDays.Count} - {loopDay}");
                }

                var daysBack = frozenNow.DayNumber - loopDay.DayNumber;

                var url = $"https://swd.weatherflow.com/swd/rest/observations/device/{deviceId}?day_offset={daysBack}";

                string observations;

                try
                {
                    observations = await client.GetStringAsync(url);
                }
                catch (Exception e)
                {
                    //Todo: User alert needed in case this represents an api change
                    //Reading online suggests that devs have been surprised by changes to the api - I like log
                    //and continue as a strategy since network failures and api outages will always create some
                    //failures - but how to get a good notification to a user?
                    Log.ForContext("url", url).Error(e, "WeatherFlow Tempest API had a null return for {startDate}",
                        loopDay);
                    continue;
                }

                var observationsJson = JsonSerializer.Deserialize<DeviceObservationsResponse>(observations);


                if (observationsJson == null)
                {
                    Log.ForContext("url", url)
                        .Warning("WeatherFlow Tempest API had a null return for {startDate}", loopDay);
                    continue;
                }

                if (observationsJson.obs == null)
                {
                    Log.ForContext("url", url)
                        .Warning("WeatherFlow Tempest API had no observations for {startDate}", loopDay);
                    continue;
                }

                var translatedObservations =
                    new TempestDayObservations(loopDay, deviceId, deviceStationResults, observationsJson.obs.ToList());

                returnList.Add((translatedObservations, observations));

                Log.Verbose("WeatherFlow Tempest API downloaded information for {startDate}", loopDay);
            }

            return returnList;
        }

        public static async Task<(StationsResponse? stationResult, string stationJson)> GetStation(int stationId,
            string accessToken)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var stationJson = await client.GetStringAsync(
                $"https://swd.weatherflow.com/swd/rest/stations/{stationId}");

            var station = JsonSerializer.Deserialize<StationsResponse>(stationJson);

            return (station, stationJson);
        }

        public static async Task<(StationsResponse? stationReturn, string stationsJson)> GetStations(string accessToken)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var stationsJson = await client.GetStringAsync(
                "https://swd.weatherflow.com/swd/rest/stations");

            var stations = JsonSerializer.Deserialize<StationsResponse>(stationsJson);

            return (stations, stationsJson);
        }
    }
}