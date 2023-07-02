namespace Replybot.Models;

public class DiscordSettings
{
    public DiscordSettings(string botToken, string avatarBaseUrl, int maxCharacters)
    {
        BotToken = botToken;
        AvatarBaseUrl = avatarBaseUrl;
        MaxCharacters = maxCharacters;
    }

    public string BotToken { get; set; }
    public string AvatarBaseUrl { get; set; }
    public int MaxCharacters { get; set; }
}