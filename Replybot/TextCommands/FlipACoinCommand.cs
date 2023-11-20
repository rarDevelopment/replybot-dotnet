using Replybot.BusinessLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class FlipACoinCommand(IReplyBusinessLayer replyBusinessLayer) : ITextCommand
{
    private readonly string[] _triggers = { "flip a coin", "🪙", "heads or tails" };
    private readonly string[] _replies = { "Heads 🪙", "Tails 🪙" };

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
            NotifyWhenReplying = true,
        });
    }
}