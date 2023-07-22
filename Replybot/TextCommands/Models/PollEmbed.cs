namespace Replybot.TextCommands.Models;

public class PollEmbed
{
    public PollEmbed(Embed embed, IReadOnlyList<IEmote>? reactionEmotes = null)
    {
        Embed = embed;
        ReactionEmotes = reactionEmotes;
    }

    public Embed Embed { get; set; }
    public IReadOnlyList<IEmote>? ReactionEmotes { get; set; }
}