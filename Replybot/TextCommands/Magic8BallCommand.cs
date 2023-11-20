using Replybot.BusinessLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class Magic8BallCommand(IReplyBusinessLayer replyBusinessLayer) : ITextCommand
{
    private readonly string[] _triggers = { "8-ball", "8ball", "8 ball", "🎱" };
    private readonly string[] _replies = { "It is certain.",
        "It is decidedly so.",
        "Without a doubt.",
        "Yes, definitely",
        "You can count on it.",
        "As I see it, yes.",
        "Most likely.",
        "Outlook good.",
        "Yes.",
        "Signs point to yes.",
        "Reply hazy, try again.",
        "Ask again later.",
        "Better not tell you now.",
        "Cannot predict now.",
        "Concentrate and ask again.",
        "Don't count on it.",
        "My reply is no.",
        "My sources say no.",
        "Outlook not so good.",
        "Very doubtful.",
        "Better believe it!" };

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
            Reactions = new List<IEmote> { new Emoji("🎱") },
            StopProcessing = true,
            NotifyWhenReplying = true,
        });
    }
}