namespace Replybot.Models;

public class DiscordSettings
{
    public DiscordSettings(string botToken, string avatarBaseUrl, int maxCharacters, string baseUrl)
    {
        BotToken = botToken;
        AvatarBaseUrl = avatarBaseUrl;
        MaxCharacters = maxCharacters;
        BaseUrl = baseUrl;
    }

    public string BotToken { get; set; }
    public string AvatarBaseUrl { get; set; }
    public int MaxCharacters { get; set; }
    public string BaseUrl { get; set; }
}