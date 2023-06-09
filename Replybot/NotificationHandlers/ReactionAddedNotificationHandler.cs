using MediatR;
using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.Notifications;
using Replybot.TextCommands;

namespace Replybot.NotificationHandlers;

public class ReactionAddedNotificationHandler : INotificationHandler<ReactionAddedNotification>
{
    private readonly IGuildConfigurationBusinessLayer _configurationBusinessLayer;
    private readonly KeywordHandler _keywordHandler;
    private readonly FixTwitterCommand _fixTwitterCommand;
    private readonly FixInstagramCommand _fixInstagramCommand;

    public ReactionAddedNotificationHandler(IGuildConfigurationBusinessLayer configurationBusinessLayer,
        KeywordHandler keywordHandler,
        FixTwitterCommand fixTwitterCommand,
        FixInstagramCommand fixInstagramCommand)
    {
        _configurationBusinessLayer = configurationBusinessLayer;
        _keywordHandler = keywordHandler;
        _fixTwitterCommand = fixTwitterCommand;
        _fixInstagramCommand = fixInstagramCommand;
    }
    public Task Handle(ReactionAddedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var reaction = notification.Reaction;
            var user = reaction.User.GetValueOrDefault();
            var message = await notification.Message.GetOrDownloadAsync();

            if (user is IGuildUser { IsBot: true })
            {
                return Task.CompletedTask;
            }

            bool fixingTwitter = Equals(reaction.Emote, _fixTwitterCommand.GetFixTwitterEmote());
            bool fixingInstagram = Equals(reaction.Emote, _fixInstagramCommand.GetFixInstagramEmote());
            if (!fixingTwitter && !fixingInstagram)
            {
                return Task.CompletedTask;
            }

            if (message == null)
            {
                return Task.CompletedTask;
            }

            ReactionMetadata? fixReaction = null;

            if (fixingTwitter)
            {
                fixReaction = message.Reactions.FirstOrDefault(r => Equals(r.Key, _fixTwitterCommand.GetFixTwitterEmote())).Value;
            }

            if (fixingInstagram)
            {
                fixReaction = message.Reactions.FirstOrDefault(r => Equals(r.Key, _fixInstagramCommand.GetFixInstagramEmote())).Value;
            }

            if (fixReaction == null)
            {
                return Task.CompletedTask;
            }

            if (fixReaction.Value.ReactionCount > 2)
            {
                return Task.CompletedTask;
            }
            if (notification.Reaction.Channel is not IGuildChannel guildChannel)
            {
                return Task.CompletedTask;
            }

            var config = await _configurationBusinessLayer.GetGuildConfiguration(guildChannel.Guild);
            if (fixingTwitter && config is { EnableFixTweetReactions: false } || fixingInstagram && config is { EnableFixInstagramReactions: false })
            {
                return Task.CompletedTask;
            }

            TriggerKeyword? keywordToPass = null;

            if (fixingTwitter)
            {
                if (_fixTwitterCommand.DoesMessageContainTwitterUrl(message))
                {
                    keywordToPass = TriggerKeyword.FixTwitter;
                }

                else if (_fixTwitterCommand.DoesMessageContainFxTwitterUrl(message))
                {
                    keywordToPass = TriggerKeyword.BreakTwitter;
                }
            }

            if (fixingInstagram)
            {
                if (_fixInstagramCommand.DoesMessageContainInstagramUrl(message))
                {
                    keywordToPass = TriggerKeyword.FixInstagram;
                }

                else if (_fixInstagramCommand.DoesMessageContainDdInstagramUrl(message))
                {
                    keywordToPass = TriggerKeyword.BreakInstagram;
                }
            }

            if (keywordToPass == null)
            {
                return Task.CompletedTask;
            }

            (string fixedMessage, MessageReference messageToReplyTo)? fixedMessage = null;

            if (fixingTwitter)
            {
                fixedMessage = await _fixTwitterCommand.GetFixedTwitterMessage(message, keywordToPass.Value);
                if (fixedMessage == null || fixedMessage.Value.fixedMessage ==
                    _fixTwitterCommand.NoLinkMessage)
                {
                    return Task.CompletedTask;
                }
            }
            else if (fixingInstagram)
            {
                fixedMessage = await _fixInstagramCommand.GetFixedInstagramMessage(message, keywordToPass.Value);

                if (fixedMessage == null || fixedMessage.Value.fixedMessage == _fixInstagramCommand.NoLinkMessage)
                {
                    return Task.CompletedTask;
                }
            }

            if (fixedMessage == null)
            {
                return Task.CompletedTask;
            }

            await message.ReplyAsync(fixedMessage.Value.fixedMessage);
            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}