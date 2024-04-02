using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.BusinessLayer.Extensions;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class GetGuildIconCommand(IReplyBusinessLayer replyBusinessLayer,
        IDiscordFormatter discordFormatter)
    : ITextCommand
{
    private readonly string[] _triggers = ["server icon"];

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

        return Task.FromResult(new CommandResponse
        {
            Description = guild.IconUrl.GetImageUrlWithCorrectFileExtension("No icon."),
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = true,
        });
    }
}