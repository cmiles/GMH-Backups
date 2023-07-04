namespace GmhWorkshop.SensorPush;

public static class SensorPushCommonTools
{
    /// <summary>
    ///     Returns an authorized SensorPush client. This DOES NOT have any hooks/logic/code
    ///     to deal with token renewal - only expect this to work without modification
    ///     for short lived clients!!
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static async Task<Client> AuthorizedClient(string email, string password)
    {
        var authClient = new Client(new HttpClient());
        var authR = await authClient.AuthorizeAsync(new AuthorizeRequest
            { Email = email, Password = password });
        var oAuth = await authClient.AccessTokenAsync(
            new AccessTokenRequest { Authorization = authR.Authorization });

        var httpClientWithAuthorization = new HttpClient();
        httpClientWithAuthorization.DefaultRequestHeaders.Add("Authorization", oAuth.Accesstoken);

        return new Client(httpClientWithAuthorization);
    }
}