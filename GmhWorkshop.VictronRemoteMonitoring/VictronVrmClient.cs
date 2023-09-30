using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;

namespace GmhWorkshop.VictronRemoteMonitoring;
public static class VictronVrmTools
{
    public static readonly string VictronApiUrl = "https://vrmapi.victronenergy.com/v2/";
    public static async Task<AuthResponse> Login(string username, string password)
    {
        var login = VictronApiUrl.AppendPathSegment("auth/login");

        var response = await login.WithHeader("Content-Type", "application/json").PostJsonAsync(new { username = "username", password = "password" });

        return await response.GetJsonAsync<AuthResponse>();
    }

    public static async Task<UserResponse> GetLoggedInUser(string token)
    {
        var userInfoResponse = await VictronApiUrl.AppendPathSegment("users/me").WithXAuthBearerToken(token).GetAsync();
        var response = await userInfoResponse.GetJsonAsync<UserResponse>();

        return response;
    }

    public static async Task<InstallationsResponse> GetInstallations(string token, string userId)
    {
        var installationsResponse = await VictronApiUrl.AppendPathSegment($"users/{userId}/installations").WithXAuthBearerToken(token).GetAsync();
        var response = await installationsResponse.GetJsonAsync<InstallationsResponse>();

        return response;
    }

    public static IFlurlRequest WithXAuthBearerToken(this Url url, string token)
    {
        return new FlurlRequest(url).WithHeader("x-authorization", $"Bearer {token}");
    }
}