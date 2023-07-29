using MediatR;
using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.Notifications;
using Replybot.ReactionCommands;

namespace Replybot.NotificationHandlers;

public class ReactionAddedNotificationHandler : INotificationHandler<ReactionAddedNotification>
{
    private readonly IGuildConfigurationBusinessLayer _configurationBusinessLayer;
    private readonly IEnumerable<IReactionCommand> _reactionCommands;

    public ReactionAddedNotificationHandler(
        IGuildConfigurationBusinessLayer configurationBusinessLayer,
        IEnumerable<IReactionCommand> reactionCommands)
    {
        _configurationBusinessLayer = configurationBusinessLayer;
        _reactionCommands = reactionCommands;
    }

    public Task Handle(ReactionAddedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var reaction = notification.Reaction;
            var reactingUser = reaction.User.GetValueOrDefault();
            var message = await notification.Message.GetOrDownloadAsync();

            if (reactingUser is IGuildUser { IsBot: true } ||
                message == null ||
                notification.Reaction.Channel is not IGuildChannel guildChannel)
            {
                return Task.CompletedTask;
            }

            var config = await _configurationBusinessLayer.GetGuildConfiguration(guildChannel.Guild);
            if (config == null)
            {
                return Task.CompletedTask;
            }

            foreach (var reactionCommand in _reactionCommands)
            {
                await ProcessReactions(reactionCommand, reaction, config, message, reactingUser);
            }

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }

    private static async Task ProcessReactions(IReactionCommand reactCommand,
        IReaction reaction,
        GuildConfiguration config,
        IUserMessage message,
        IUser reactingUser)
    {
        ReactionMetadata? fixReaction = null;
        if (reactCommand.IsReacting(reaction.Emote, config))
        {
            fixReaction = message.Reactions.FirstOrDefault(r => reactCommand.IsReacting(r.Key, config)).Value;
        }

        if (fixReaction == null || fixReaction.Value.ReactionCount > 2)
        {
            return;
        }

        var commandResponses = await reactCommand.HandleMessage(message, reactingUser);
        foreach (var commandResponse in commandResponses)
        {
            var allowedMentions = new AllowedMentions
            {
                AllowedTypes = AllowedMentionTypes.Users | AllowedMentionTypes.Roles | AllowedMentionTypes.Everyone,
                MentionRepliedUser = commandResponse.NotifyWhenReplying
            };
            if (commandResponse.FileAttachments.Any())
            {
                await message.Channel.SendFilesAsync(commandResponse.FileAttachments, commandResponse.Description,
                    messageReference: new MessageReference(message.Id, failIfNotExists: false), allowedMentions: allowedMentions);
            }
            else
            {
                await message.ReplyAsync(commandResponse.Description, allowedMentions: allowedMentions);
            }
        }
    }
}