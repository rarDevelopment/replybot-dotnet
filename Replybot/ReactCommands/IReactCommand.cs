using Replybot.Models;
using Replybot.TextCommands;

namespace Replybot.ReactCommands;

public interface IReactCommand
{
    bool CanHandle(string message, GuildConfiguration configuration);
    Task<List<Emote>> HandleReact(SocketMessage message);
    bool IsReacting(IEmote reactionEmote, GuildConfiguration guildConfiguration);
    Task<List<CommandResponse>> HandleMessage(IUserMessage message);
}