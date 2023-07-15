using Replybot.Models;

namespace Replybot.ReactionCommands;

public interface IReactionCommand
{
    bool CanHandle(string message, GuildConfiguration configuration);
    Task<List<Emote>> HandleReaction(SocketMessage message);
    bool IsReacting(IEmote reactionEmote, GuildConfiguration guildConfiguration);
    Task<List<CommandResponse>> HandleMessage(IUserMessage message);
}