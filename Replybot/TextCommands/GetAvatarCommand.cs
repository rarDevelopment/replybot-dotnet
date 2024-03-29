﻿using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class GetAvatarCommand(IReplyBusinessLayer replyBusinessLayer,
        IDiscordFormatter discordFormatter)
    : ITextCommand
{
    private readonly string[] _triggers = { "server avatar", "avatar" };

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

        var userAvatars = message.MentionedUsers.Any()
            ? message.MentionedUsers.Select(u => GetUserAvatarUrl(u, message.Content))
            : new List<string>
            {
                GetUserAvatarUrl(message.Author, message.Content)
            };

        return Task.FromResult(new CommandResponse
        {
            Description = string.Join("\n", userAvatars),
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = true,
        });
    }

    private static string GetUserAvatarUrl(IUser user, string messageContent)
    {
        return messageContent.Contains("server", StringComparison.CurrentCultureIgnoreCase)
            ? (user as IGuildUser)?.GetDisplayAvatarUrl() ?? user.GetAvatarUrl(ImageFormat.Png)
            : user.GetAvatarUrl(ImageFormat.Png);
    }
}