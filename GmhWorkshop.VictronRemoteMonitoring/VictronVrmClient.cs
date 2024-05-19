using Flurl;
using Flurl.Http;
using GmhWorkshop.VictronRemoteMonitoring.Models;

namespace GmhWorkshop.VictronRemoteMonitoring;

public static class VictronVrmTools
{
    public static readonly string VictronApiUrl = "https://vrmapi.victronenergy.com/v2/";

    public static async Task<InstallationsResponse> GetInstallations(string token, string userId)
    {
        var installationsResponse = await VictronApiUrl.AppendPathSegment($"users/{userId}/installations")
            .WithXAuthBearerToken(token).GetAsync();
        var response = await installationsResponse.GetJsonAsync<InstallationsResponse>();

        return response;
    }

    public static async Task<UserResponse> GetLoggedInUser(string token)
    {
        var userInfoResponse = await VictronApiUrl.AppendPathSegment("users/me").WithXAuthBearerToken(token).GetAsync();
        var response = await userInfoResponse.GetJsonAsync<UserResponse>();

        return response;
    }

    public static async Task<StatsResponse> GetStats(string token, string installationId)
    {
        var statsResponse = await "https://vrmapi.victronenergy.com/v2/"
            .AppendPathSegment($"installations/{installationId}/stats").SetQueryParams(new
            {
                interval = "hours",
                show_instance = true,
                type = new[] { "solar_yield" }
            }).WithXAuthBearerToken(token).GetAsync();
        var response = await statsResponse.GetJsonAsync<StatsResponse>();

        return response;
    }

    public static async Task<AuthResponse> Login(string username, string password)
    {
        var login = VictronApiUrl.AppendPathSegment("auth/login");

        var response = await login.WithHeader("Content-Type", "application/json")
            .PostJsonAsync(new { username = "username", password = "password" });

        return await response.GetJsonAsync<AuthResponse>();
    }

    public static IFlurlRequest WithXAuthBearerToken(this Url url, string token)
    {
        return new FlurlRequest(url).WithHeader("x-authorization", $"Bearer {token}");
    }
}