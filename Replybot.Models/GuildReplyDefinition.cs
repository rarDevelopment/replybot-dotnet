namespace Replybot.Models;

public class GuildReplyDefinition
{
    public GuildReplyDefinition(ulong guildId, string[] triggers,
        string[]? replies,
        UserReply[]? userReplies,
        bool mentionAuthor,
        bool requiresBotName,
        string[]? reactions, decimal priority, bool isSpecialFeature)
    {
        GuildId = guildId;
        Triggers = triggers;
        Replies = replies;
        UserReplies = userReplies;
        MentionAuthor = mentionAuthor;
        RequiresBotName = requiresBotName;
        Reactions = reactions;
        Priority = priority;
        IsSpecialFeature = isSpecialFeature;
    }
    public string? Id { get; set; }
    public ulong GuildId { get; set; }
    public string[] Triggers { get; set; }
    public string[]? Replies { get; set; }
    public UserReply[]? UserReplies { get; set; }
    public bool MentionAuthor { get; set; }
    public bool RequiresBotName { get; set; }
    public string[]? Reactions { get; set; }
    public decimal Priority { get; set; }
    public bool IsDefaultReply => GuildId == 0;
    public bool IsSpecialFeature { get; set; }
}