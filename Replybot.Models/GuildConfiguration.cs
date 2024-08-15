namespace Replybot.Models;

public class GuildConfiguration
{
    public string GuildId { get; set; }
    public string? GuildName { get; set; }
    public bool EnableAvatarAnnouncements { get; set; }
    public bool EnableAvatarMentions { get; set; }
    public string? LogChannelId { get; set; }
    public List<string> AdminUserIds { get; set; }
    public bool EnableDefaultReplies { get; set; }
    public bool EnableFixTweetReactions { get; set; }
    public bool EnableFixInstagramReactions { get; set; }
    public bool EnableFixRedditReactions { get; set; }
    public bool EnableFixBlueskyReactions { get; set; }
    public bool EnableFixTikTokReactions { get; set; }
    public List<string> IgnoreAvatarChangesUserIds { get; set; }
    public bool EnableWelcomeMessage { get; set; }
}