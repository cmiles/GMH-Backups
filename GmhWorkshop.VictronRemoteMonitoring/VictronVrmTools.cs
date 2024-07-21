using System.Text.Json;
using Flurl;
using Flurl.Http;
using GmhWorkshop.VictronRemoteMonitoring.ApiDtos;
using GmhWorkshop.VictronRemoteMonitoring.Models;
using PointlessWaymarks.CommonTools;
using Serilog;

namespace GmhWorkshop.VictronRemoteMonitoring;

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