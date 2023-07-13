using Replybot.Models;

namespace Replybot.TextCommands;

public interface IReactCommand
{
    bool CanHandle(string message, GuildConfiguration configuration);
    Task<List<Emote>> Handle(SocketMessage message);
}