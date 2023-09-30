namespace GmhWorkshop.VictronRemoteMonitoring;

public class AuthResponse
{
    public string token { get; set; }
    public int idUser { get; set; }
    public string verification_mode { get; set; }
    public bool verification_sent { get; set; }
}
