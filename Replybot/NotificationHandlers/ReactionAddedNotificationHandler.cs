using MediatR;
using Replybot.BusinessLayer;
using Replybot.Notifications;
using Replybot.ReactionCommands;

namespace Replybot.NotificationHandlers;

public class ReactionAddedNotificationHandler : INotificationHandler<ReactionAddedNotification>
{
    private readonly IGuildConfigurationBusinessLayer _configurationBusinessLayer;
    private readonly IEnumerable<IReactionCommand> _reactCommands;

    public ReactionAddedNotificationHandler(
        IGuildConfigurationBusinessLayer configurationBusinessLayer,
        IEnumerable<IReactionCommand> reactCommands)
    {
        _configurationBusinessLayer = configurationBusinessLayer;
        _reactCommands = reactCommands;
    }

    public Task Handle(ReactionAddedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var reaction = notification.Reaction;
            var user = reaction.User.GetValueOrDefault();
            var message = await notification.Message.GetOrDownloadAsync();

            if (user is IGuildUser { IsBot: true } ||
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

            foreach (var reactCommand in _reactCommands)
            {
                ReactionMetadata? fixReaction = null;
                if (reactCommand.IsReacting(reaction.Emote, config))
                {
                    fixReaction = message.Reactions.FirstOrDefault(r => reactCommand.IsReacting(r.Key, config)).Value;
                }

                if (fixReaction == null || fixReaction.Value.ReactionCount > 2)
                {
                    continue;
                }

                var messagesToSend = await reactCommand.HandleMessage(message);
                foreach (var messageToSend in messagesToSend)
                {
                    if (messageToSend.FileAttachments.Any())
                    {
                        await message.Channel.SendFilesAsync(messageToSend.FileAttachments, messageToSend.Description,
                            messageReference: new MessageReference(message.Id, failIfNotExists: false));
                    }
                    else
                    {
                        await message.ReplyAsync(messageToSend.Description);
                    }
                }
            }

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}