namespace Replybot.Models;

public class ApplicationEmojiSettings(string fixTwitter, string fixInstagram, string fixTikTok, string fixBluesky, string fixReddit)
{
    public string FixTwitter { get; set; } = fixTwitter;
    public string FixInstagram { get; set; } = fixInstagram;
    public string FixTikTok { get; set; } = fixTikTok;
    public string FixBluesky { get; set; } = fixBluesky;
    public string FixReddit { get; set; } = fixReddit;
}