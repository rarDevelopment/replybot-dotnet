namespace Replybot.Models;

public class GuildReplyDefinition(ulong guildId, string[] triggers,
    string[]? replies,
    string[]? channelIds,
    string[]? userIds,
    bool mentionAuthor,
    bool requiresBotName,
    string[]? reactions,
    decimal priority,
    bool isActive)
{
    public string? Id { get; set; }
    public ulong GuildId { get; set; } = guildId;
    public string[] Triggers { get; set; } = triggers;
    public string[]? Replies { get; set; } = replies;
    public string[]? ChannelIds { get; set; } = channelIds;
    public string[]? UserIds { get; set; } = userIds;
    public bool MentionAuthor { get; set; } = mentionAuthor;
    public bool RequiresBotName { get; set; } = requiresBotName;
    public string[]? Reactions { get; set; } = reactions;
    public decimal Priority { get; set; } = priority;
    public bool IsActive { get; set; } = isActive;
    public bool IsDefaultReply => GuildId == 0;
}