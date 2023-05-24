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

    public ReactionAddedNotificationHandler(IGuildConfigurationBusinessLayer configurationBusinessLayer,
        KeywordHandler keywordHandler,
        FixTwitterCommand fixTwitterCommand)
    {
        _configurationBusinessLayer = configurationBusinessLayer;
        _keywordHandler = keywordHandler;
        _fixTwitterCommand = fixTwitterCommand;
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

            if (!Equals(reaction.Emote, _fixTwitterCommand.GetFixTwitterEmote()))
            {
                return Task.CompletedTask;
            }

            if (message == null)
            {
                return Task.CompletedTask;
            }

            var fixTwitterReaction = message.Reactions
                .FirstOrDefault(r => Equals(r.Key, _fixTwitterCommand.GetFixTwitterEmote())).Value;
            if (fixTwitterReaction.ReactionCount > 2)
            {
                return Task.CompletedTask;
            }
            if (notification.Reaction.Channel is not IGuildChannel guildChannel)
            {
                return Task.CompletedTask;
            }

            var config = await _configurationBusinessLayer.GetGuildConfiguration(guildChannel.Guild);
            if (config is { EnableFixTweetReactions: false })
            {
                return Task.CompletedTask;
            }

            TriggerKeyword? keywordToPass = null;

            if (_fixTwitterCommand.DoesMessageContainTwitterUrl(message))
            {
                keywordToPass = TriggerKeyword.FixTwitter;
            }

            else if (_fixTwitterCommand.DoesMessageContainFxTwitterUrl(message))
            {
                keywordToPass = TriggerKeyword.BreakTwitter;
            }

            if (keywordToPass == null)
            {
                return Task.CompletedTask;
            }

            var fixedTwitterMessage = await _fixTwitterCommand.GetFixedTwitterMessage(message, keywordToPass.Value);

            if (fixedTwitterMessage == null || fixedTwitterMessage.Value.fixedTwitterMessage ==
                _fixTwitterCommand.NoLinkMessage)
            {
                return Task.CompletedTask;
            }

            await message.ReplyAsync(fixedTwitterMessage.Value.fixedTwitterMessage);
            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}