namespace Replybot.TextCommands;

public interface ITextCommand
{
    bool CanHandle(string? reply);
    Task<CommandResponse> Handle(SocketMessage message);
}