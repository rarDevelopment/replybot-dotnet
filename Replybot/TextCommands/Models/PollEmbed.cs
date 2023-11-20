namespace Replybot.TextCommands.Models;

public class PollEmbed(Embed embed, IReadOnlyList<IEmote>? reactionEmotes = null)
{
    public Embed Embed { get; set; } = embed;
    public IReadOnlyList<IEmote>? ReactionEmotes { get; set; } = reactionEmotes;
}