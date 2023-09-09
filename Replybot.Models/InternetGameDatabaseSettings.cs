namespace Replybot.Models;

public class InternetGameDatabaseSettings
{
    public InternetGameDatabaseSettings(string clientId, string clientSecret)
    {
        ClientId = clientId;
        ClientSecret = clientSecret;
    }

    public string ClientId { get; }
    public string ClientSecret { get; }
}