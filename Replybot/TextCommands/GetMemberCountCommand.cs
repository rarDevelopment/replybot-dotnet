using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class GetMemberCountCommand(IReplyBusinessLayer replyBusinessLayer,
        IDiscordFormatter discordFormatter)
    : ITextCommand
{
    private readonly string[] _triggers = ["how many members", "how many people", "member count", "people count", "headcount", "head count"];

    public bool CanHandle(TextCommandReplyCriteria replyCriteria)
    {
        return replyCriteria.IsBotNameMentioned &&
               _triggers.Any(t => replyBusinessLayer.GetWordMatch(t, replyCriteria.MessageText));
    }

    public Task<CommandResponse> Handle(SocketMessage message)
    {
        if (message.Channel is not IGuildChannel { Guild: SocketGuild guild })
        {
            return Task.FromResult(new CommandResponse
            {
                Embed = discordFormatter.BuildErrorEmbedWithUserFooter("Not a Server",
                    "This command can only be used in a Discord server, it will not work in a DM.", message.Author),
                StopProcessing = true,
                NotifyWhenReplying = true,
            });
        }

        var embed = discordFormatter.BuildRegularEmbedWithUserFooter("Server Member Count",
            $"This server has **{guild.MemberCount}** members.", message.Author);

        return Task.FromResult(new CommandResponse
        {
            Embed = embed,
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = true,
        });
    }
}