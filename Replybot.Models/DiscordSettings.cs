namespace Replybot.Models;

public class DiscordSettings(string botToken, string avatarBaseUrl, int maxCharacters, string baseUrl)
{
    public string BotToken { get; set; } = botToken;
    public string AvatarBaseUrl { get; set; } = avatarBaseUrl;
    public int MaxCharacters { get; set; } = maxCharacters;
    public string BaseUrl { get; set; } = baseUrl;
}