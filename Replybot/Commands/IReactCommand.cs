using Replybot.Models;

namespace Replybot.Commands;

public interface IReactCommand
{
    bool CanHandle(string message, GuildConfiguration configuration);
    Task<List<Emote>> HandleReact(SocketMessage message);
    bool IsReacting(IEmote reactionEmote, GuildConfiguration guildConfiguration);
    Task<List<MessageToSend>> HandleMessage(IUserMessage message);
}