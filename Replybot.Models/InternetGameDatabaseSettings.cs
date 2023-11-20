namespace Replybot.Models;

public class InternetGameDatabaseSettings(string clientId, string clientSecret)
{
    public string ClientId { get; } = clientId;
    public string ClientSecret { get; } = clientSecret;
}