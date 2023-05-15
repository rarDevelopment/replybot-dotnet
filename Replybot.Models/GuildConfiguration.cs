namespace Replybot.Models;

public class GuildConfiguration
{
    public string GuildId { get; set; }
    public string? GuildName { get; set; }
    public bool EnableAvatarAnnouncements { get; set; }
    public bool EnableAvatarMentions { get; set; }
    public string? LogChannelId { get; set; }
    public List<string> AdminUserIds { get; set; }
    public bool EnableAutoFixTweets { get; set; }
    public bool EnableAutoBreakTweets { get; set; }
    public bool EnableDefaultReplies { get; set; }
    public bool IsSpecialFeature { get; set; }
}