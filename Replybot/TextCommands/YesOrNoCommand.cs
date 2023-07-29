using Replybot.BusinessLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class YesOrNoCommand : ITextCommand
{
    private readonly IReplyBusinessLayer _replyBusinessLayer;
    private readonly string[] _triggers = { "yes or no", "yes/no", "yes / no", "y/n", "y/n", "y or n" };
    private readonly string[] _replies = { "Yes 👍🏼", "No 👎🏼" };

    public YesOrNoCommand(IReplyBusinessLayer replyBusinessLayer)
    {
        _replyBusinessLayer = replyBusinessLayer;
    }

    public bool CanHandle(TextCommandReplyCriteria replyCriteria)
    {
        return replyCriteria.IsBotNameMentioned &&
               _triggers.Any(t => _replyBusinessLayer.GetWordMatch(t, replyCriteria.MessageText));
    }

    public Task<CommandResponse> Handle(SocketMessage message)
    {
        return Task.FromResult(new CommandResponse
        {
            Description = _replyBusinessLayer.ChooseReply(_replies),
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = true
        });
    }
}