namespace Replybot.TextCommands;

public interface IReplyCommand
{
    bool CanHandle(string? reply);
    Task<MessageToSend> Handle(SocketMessage message);
}