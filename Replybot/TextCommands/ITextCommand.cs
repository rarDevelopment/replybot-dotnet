namespace Replybot.TextCommands;

public interface ITextCommand
{
    bool CanHandle(TextCommandReplyCriteria replyCriteria);
    Task<CommandResponse> Handle(SocketMessage message);
}