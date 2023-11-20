using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class VersionCommand(IReplyBusinessLayer replyBusinessLayer, VersionSettings versionSettings)
    : ITextCommand
{
    private readonly string[] _triggers = { "-v", "version", "what version" };
    private const string VersionNumberKeyword = "{{VERSIONNUMBER}}";
    private readonly string[] _replies = {
        $"I am {VersionNumberKeyword} versions old.",
        $"That's kind of private, but if you must know, I'm at v{VersionNumberKeyword}.",
        $"I'm at v{VersionNumberKeyword}, what version are you at?"
    };

    public bool CanHandle(TextCommandReplyCriteria replyCriteria)
    {
        return replyCriteria.IsBotNameMentioned &&
               _triggers.Any(t => replyBusinessLayer.GetWordMatch(t, replyCriteria.MessageText));
    }

    public Task<CommandResponse> Handle(SocketMessage message)
    {
        return Task.FromResult(new CommandResponse
        {
            Description = replyBusinessLayer.ChooseReply(_replies)?.Replace(VersionNumberKeyword, versionSettings.VersionNumber),
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = true,
        });
    }
}