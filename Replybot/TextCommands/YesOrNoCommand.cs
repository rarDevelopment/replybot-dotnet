using Replybot.BusinessLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class YesOrNoCommand(IReplyBusinessLayer replyBusinessLayer) : ITextCommand
{
    private readonly string[] _triggers = { "yes or no", "yes/no", "yes / no", "y/n", "y/n", "y or n" };
    private readonly string[] _replies = { "Yes 👍🏼", "No 👎🏼" };

    public bool CanHandle(TextCommandReplyCriteria replyCriteria)
    {
        return replyCriteria.IsBotNameMentioned &&
               _triggers.Any(t => replyBusinessLayer.GetWordMatch(t, replyCriteria.MessageText));
    }

    public Task<CommandResponse> Handle(SocketMessage message)
    {
        return Task.FromResult(new CommandResponse
        {
            Description = replyBusinessLayer.ChooseReply(_replies),
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = true
        });
    }
}