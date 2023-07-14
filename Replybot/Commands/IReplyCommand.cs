namespace Replybot.Commands;

public interface IReplyCommand
{
    bool CanHandle(string? reply);
    Task<MessageToSend> Handle(SocketMessage message);
}