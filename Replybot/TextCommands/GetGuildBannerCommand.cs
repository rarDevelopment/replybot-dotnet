using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class GetGuildBannerCommand : ITextCommand
{
    private readonly IReplyBusinessLayer _replyBusinessLayer;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly string[] _triggers = { "server banner" };

    public GetGuildBannerCommand(IReplyBusinessLayer replyBusinessLayer,
        IDiscordFormatter discordFormatter)
    {
        _replyBusinessLayer = replyBusinessLayer;
        _discordFormatter = discordFormatter;
    }

    public bool CanHandle(TextCommandReplyCriteria replyCriteria)
    {
        return replyCriteria.IsBotNameMentioned &&
               _triggers.Any(t => _replyBusinessLayer.GetWordMatch(t, replyCriteria.MessageText));
    }

    public Task<CommandResponse> Handle(SocketMessage message)
    {
        if (message.Channel is not IGuildChannel { Guild: SocketGuild guild })
        {
            return Task.FromResult(new CommandResponse
            {
                Embed = _discordFormatter.BuildErrorEmbedWithUserFooter("Not a Server",
                    "This command can only be used in a Discord server, it will not work in a DM.", message.Author),
                StopProcessing = true,
                NotifyWhenReplying = true,
            });
        }

        return Task.FromResult(new CommandResponse
        {
            Description = string.IsNullOrEmpty(guild?.BannerUrl) ? "No banner." : guild.BannerUrl,
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = true,
        });
    }
}