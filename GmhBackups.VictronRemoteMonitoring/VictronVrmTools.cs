using System.Text.Json;
using Flurl;
using Flurl.Http;
using GmhBackups.VictronRemoteMonitoring.ApiDtos;
using GmhBackups.VictronRemoteMonitoring.Models;
using PointlessWaymarks.CommonTools;
using Polly;
using Serilog;

namespace GmhBackups.VictronRemoteMonitoring;

public static class VictronVrmTools
{
    public static readonly string VictronApiUrl = "https://vrmapi.victronenergy.com/v2/";

    public static async Task<InstallationsResponse> Installations(string token, string userId)
    {
        Console.WriteLine("VRM Api: Querying Installations");

        var installationsResponse = await VictronApiUrl.AppendPathSegment($"users/{userId}/installations")
            .WithXAuthBearerToken(token).GetAsync();
        var response = await installationsResponse.GetJsonAsync<InstallationsResponse>();

        Console.WriteLine($"VRM Api: Found {response.records.Length} Installations");

        return response;
    }

    public static async Task<UserResponse> LoggedInUser(string token)
    {
        Console.WriteLine("VRM Api: Querying User Info");

        var userInfoResponse = await VictronApiUrl.AppendPathSegment("users/me").WithXAuthBearerToken(token).GetAsync();
        var response = await userInfoResponse.GetJsonAsync<UserResponse>();

        return response;
    }

    public static async Task<DiagnosticsResponse> Diagnostics(string token, string installationId)
    {
        Console.WriteLine("VRM Api: Querying Diagnostics");

        var solarYieldStatsResponse = await "https://vrmapi.victronenergy.com/v2/"
            .AppendPathSegment($"installations/{installationId}/diagnostics").WithXAuthBearerToken(token).GetAsync();
        var response = await solarYieldStatsResponse.GetJsonAsync<DiagnosticsResponse>();

        Console.WriteLine($"VRM Api: Found {response.num_records} Diagnostic Records");

        return response;
    }

    public static async Task<List<VrmStat>> AllStatsFromDiagnostics(string token, string installationId,
        DateTime startUtc, DateTime endUtc)
    {
        var diagnostics = await Diagnostics(token, installationId);

        var stats = new List<StatsResponse>();

        Console.WriteLine($"VRM Api: Querying Stats for {diagnostics.records.Length} Stat Codes");

        var codeCount = 0;

        Console.WriteLine($"VRM Api: Querying Stats Groups - {++codeCount} of {diagnostics.records.Length}");

        var statsResponse = await Stats(token, installationId,
            diagnostics.records.Select(x => x.code).Distinct().ToList(),
            startUtc, endUtc);

        if (statsResponse == null)
        {
            throw new Exception("VRM API StatsResponse was null");
        }

        if (statsResponse.Records.Count == 0)
        {
        }

        stats.Add(statsResponse);

        var returnList = new List<VrmStat>();

        Console.WriteLine($"VRM Api: Converting {stats.Count} Stats Responses to VrmStats Format");

        foreach (var loopStats in stats)
        {
            returnList.AddRange(loopStats.ToVrmStats(diagnostics.records.ToList()));
        }

        Console.WriteLine($"VRM Api: Returning {returnList.Count} Stats");

        return returnList;
    }

    /// <summary>
    /// Query the VRM API for Stats - first tries to query all stats at once, if that fails it will query each stat one
    /// at a time - the one at a time query incorporates a 2-second delay between each query which will significantly
    /// extend the runtime.
    /// </summary>
    /// <param name="token"></param>
    /// <param name="installationId"></param>
    /// <param name="statCodes"></param>
    /// <param name="startUtc"></param>
    /// <param name="endUtc"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<StatsResponse?> Stats(string token, string installationId, List<string> statCodes,
        DateTime startUtc, DateTime endUtc)
    {
        if (statCodes.Count == 0)
        {
            throw new ArgumentException("statCodes must contain at least one element", nameof(statCodes));
        }

        var queryParams = new List<KeyValuePair<string, string>>
        {
            new("show_instance", "1"),
            new("type", "custom"),
            new("start", ((DateTimeOffset)startUtc).ToUnixTimeSeconds().ToString()),
            new("end", ((DateTimeOffset)endUtc).ToUnixTimeSeconds().ToString())
        };

        statCodes.ForEach(code => queryParams.Add(new KeyValuePair<string, string>("attributeCodes[]", code)));

        Console.WriteLine($"VRM Api: Querying Stats for {statCodes.Count} Stat Codes");

        string responseString;
        IFlurlResponse solarYieldStatsResponse;

        try
        {
            solarYieldStatsResponse = await "https://vrmapi.victronenergy.com/v2/"
                .AppendPathSegment($"installations/{installationId}/stats").SetQueryParams(queryParams)
                .WithXAuthBearerToken(token).GetAsync();

            responseString = await solarYieldStatsResponse.GetStringAsync();
        }
        catch (Exception e)
        {
            if(e is FlurlHttpException { StatusCode: 400 })
            {
                return await StatsSingleQueries(token, installationId, statCodes, startUtc, endUtc);
            }
            Console.WriteLine(e);
            throw;
        }


        Console.WriteLine("Converting Stats return to Json");

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new StatsRecordConverter());
        options.Converters.Add(new StatsTotalsConverter());

        var response = JsonSerializer.Deserialize<StatsResponse>(responseString, options);

        if (response == null)
        {
            Log.ForContext("responseStatusCode", solarYieldStatsResponse.StatusCode)
                .ForContext("responseStatusMessage", solarYieldStatsResponse.ResponseMessage)
                .ForContext(nameof(statCodes), statCodes.SafeObjectDump())
                .ForContext(nameof(responseString), responseString)
                .Error("VRM Stats didn't throw an Exception but conversion to Json Failed with a Null Return");
            return null;
        }

        if (!response.Success)
        {
            Log.ForContext("responseStatusCode", solarYieldStatsResponse.StatusCode)
                .ForContext("responseStatusMessage", solarYieldStatsResponse.ResponseMessage)
                .ForContext(nameof(statCodes), statCodes.SafeObjectDump())
                .ForContext(nameof(responseString), responseString)
                .Error("VRM Stats didn't throw an Exception but conversion to Json Failed with a Null Return");
            return null;
        }

        return response;
    }

    public static async Task<StatsResponse> StatsSingleQueries(string token, string installationId, List<string> statCodes,
    DateTime startUtc, DateTime endUtc)
    {
        if (statCodes.Count == 0)
        {
            throw new ArgumentException("statCodes must contain at least one element", nameof(statCodes));
        }

        Console.WriteLine($"VRM Api: Querying {statCodes.Count} Stats One at a Time w/Delay");

        var returnList = new List<StatsResponse>();

        var counter = 0;

        var orderedStatCodes = statCodes.OrderBy(x => x).ToList();

        foreach (var loopStat in orderedStatCodes)
        {
            Console.WriteLine($"VRM Api: Querying Stats for Stat Code {loopStat} (waiting 2s to start) - {++counter} of {orderedStatCodes.Count}");

            await Task.Delay(2000);

            var queryParams = new List<KeyValuePair<string, string>>
            {
                new("show_instance", "1"),
                new("type", "custom"),
                new("start", ((DateTimeOffset)startUtc).ToUnixTimeSeconds().ToString()),
                new("end", ((DateTimeOffset)endUtc).ToUnixTimeSeconds().ToString()),
                new("attributeCodes[]", loopStat)
            };

            string responseString;
            IFlurlResponse solarYieldStatsResponse;

            try
            {
                solarYieldStatsResponse = await "https://vrmapi.victronenergy.com/v2/"
                    .AppendPathSegment($"installations/{installationId}/stats").SetQueryParams(queryParams)
                    .WithXAuthBearerToken(token).GetAsync();

                responseString = await solarYieldStatsResponse.GetStringAsync();
            }
            catch (Exception e)
            {
                Log.Error(e, "Error Getting Stats for Stat Code {statCode}", loopStat);
                continue;
            }

            Console.WriteLine("Converting Stats return to Json");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new StatsRecordConverter());
            options.Converters.Add(new StatsTotalsConverter());

            var response = JsonSerializer.Deserialize<StatsResponse>(responseString, options);

            if (response == null)
            {
                Log.ForContext("responseStatusCode", solarYieldStatsResponse.StatusCode)
                    .ForContext("responseStatusMessage", solarYieldStatsResponse.ResponseMessage)
                    .ForContext(nameof(loopStat), loopStat)
                    .ForContext(nameof(responseString), responseString)
                    .Error("VRM Stats didn't throw an Exception for {statCode} but conversion to Json Failed with a Null Return", loopStat);
                continue;
            }

            if (!response.Success)
            {
                Log.ForContext("responseStatusCode", solarYieldStatsResponse.StatusCode)
                    .ForContext("responseStatusMessage", solarYieldStatsResponse.ResponseMessage)
                    .ForContext(nameof(loopStat), loopStat)
                    .ForContext(nameof(responseString), responseString)
                    .Error("VRM Stats didn't throw an Exception for {statCode} but conversion to Json Failed with a Null Return", loopStat);
                continue;
            }

            returnList.Add(response);
        }

        var returnStats = new StatsResponse
        {
            Records = returnList.SelectMany(x => x.Records).ToList(),
            Success = true,
            Totals = returnList.SelectMany(x => x.Totals).ToList()
        };

        return returnStats;
    }

    public static async Task<AuthResponse> Login(string username, string password)
    {
        Console.WriteLine("VRM Api: Logging In");

        var login = VictronApiUrl.AppendPathSegment("auth/login");

        var response = await login.WithHeader("Content-Type", "application/json")
            .PostJsonAsync(new { username, password });

        return await response.GetJsonAsync<AuthResponse>();
    }

    public static async Task<DeviceResponse> DeviceList(string token, string installationId)
    {
        Console.WriteLine("VRM Api: Querying Device List");

        var solarYieldStatsResponse = await "https://vrmapi.victronenergy.com/v2/"
            .AppendPathSegment($"installations/{installationId}/system-overview").WithXAuthBearerToken(token)
            .GetAsync();

        var response = await solarYieldStatsResponse.GetJsonAsync<DeviceResponse>();

        Console.WriteLine($"VRM Api: Found {response.records.devices.Length} Devices");

        return response;
    }

    public static IFlurlRequest WithXAuthBearerToken(this Url url, string token)
    {
        return new FlurlRequest(url).WithHeader("x-authorization", $"Bearer {token}");
    }
}