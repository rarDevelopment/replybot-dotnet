namespace Replybot.Commands;

public interface IReplyCommand
{
    bool CanHandle(string? reply);
    Task<CommandResponse> Handle(SocketMessage message);
}