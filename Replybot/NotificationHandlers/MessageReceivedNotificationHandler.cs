using MediatR;
using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.Notifications;
using System.Text.RegularExpressions;
using Replybot.ReactionCommands;
using Replybot.TextCommands.Models;

namespace Replybot.NotificationHandlers;
public class MessageReceivedNotificationHandler : INotificationHandler<MessageReceivedNotification>
{
    private readonly IReplyBusinessLayer _replyBusinessLayer;
    private readonly IGuildConfigurationBusinessLayer _guildConfigurationBusinessLayer;
    private readonly IEnumerable<ITextCommand> _textCommands;
    private readonly IEnumerable<IReactionCommand> _reactionCommands;
    private readonly VersionSettings _versionSettings;
    private readonly DiscordSocketClient _client;
    private readonly ExistingMessageEmbedBuilder _logMessageBuilder;
    private readonly ILogger<DiscordBot> _logger;

    public MessageReceivedNotificationHandler(IReplyBusinessLayer replyBusinessLayer,
        IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        IEnumerable<ITextCommand> textCommands,
        IEnumerable<IReactionCommand> reactionCommands,
        VersionSettings versionSettings,
        DiscordSocketClient client,
        ExistingMessageEmbedBuilder logMessageBuilder,
        ILogger<DiscordBot> logger)
    {
        _replyBusinessLayer = replyBusinessLayer;
        _guildConfigurationBusinessLayer = guildConfigurationBusinessLayer;
        _textCommands = textCommands;
        _reactionCommands = reactionCommands;
        _versionSettings = versionSettings;
        _client = client;
        _logMessageBuilder = logMessageBuilder;
        _logger = logger;
    }

    public Task Handle(MessageReceivedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var message = notification.Message;
            if (message.Author.IsBot)
            {
                return Task.CompletedTask;
            }

            var guildChannel = message.Channel as SocketGuildChannel;

            if (guildChannel != null)
            {
                await HandleDiscordMessageLink(guildChannel, notification.Message);
            }

            IGuild? guild = guildChannel?.Guild;
            var guildUsers = guild != null ? await guild.GetUsersAsync() : null;

            foreach (var command in _textCommands)
            {
                var replyCriteria = new TextCommandReplyCriteria(notification.Message.Content)
                {
                    IsBotNameMentioned = IsBotMentioned(message, guildUsers)
                };

                if (!command.CanHandle(replyCriteria))
                {
                    continue;
                }

                var commandResponse = await HandleCommandForMessage(command, message, message.Channel, new MessageReference(message.Id));
                if (commandResponse.StopProcessing)
                {
                    return Task.CompletedTask;
                }
            }

            var config = await _guildConfigurationBusinessLayer.GetGuildConfiguration(guildChannel?.Guild);
            if (config != null)
            {
                foreach (var reactionCommand in _reactionCommands)
                {
                    if (!reactionCommand.CanHandle(notification.Message.Content, config))
                    {
                        continue;
                    }

                    var reactionEmotes = await reactionCommand.HandleReaction(notification.Message);
                    foreach (var emote in reactionEmotes)
                    {
                        await message.AddReactionAsync(emote);
                    }
                }
            }

            var replyDefinition = await _replyBusinessLayer.GetReplyDefinition(message.Content,
                guildChannel?.Guild.Id.ToString(),
                message.Channel.Id.ToString(),
                message.Author.Id.ToString());

            if (replyDefinition == null)
            {
                return Task.CompletedTask;
            }

            if (replyDefinition.RequiresBotName && !IsBotMentioned(message, guildUsers))
            {
                return Task.CompletedTask;
            }

            var reply = ChooseReply(replyDefinition, message.Author);

            var wasDeleted = await HandleDelete(message, reply);
            var messageReference = wasDeleted ? null : new MessageReference(message.Id);

            //this handles skipping if the above features haven't triggered and if the default reply isn't a special feature otherwise (manually specified)
            var defaultRepliesEnabled = config?.EnableDefaultReplies ?? true;
            if (!defaultRepliesEnabled && replyDefinition is { IsDefaultReply: true, IsSpecialFeature: false })
            {
                return Task.CompletedTask;
            }

            await HandleReactions(message, replyDefinition);

            if (string.IsNullOrEmpty(reply))
            {
                return Task.CompletedTask;
            }

            var messageText = KeywordHandler.ReplaceKeywords(reply,
                message.Author.Username,
                message.Author.Id,
                _versionSettings.VersionNumber,
                message.Content,
                replyDefinition,
                message.MentionedUsers.ToList(),
                guildChannel?.Guild);

            await message.Channel.SendMessageAsync(
                messageText,
                messageReference: messageReference
            );

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }

    private static async Task<CommandResponse> HandleCommandForMessage(ITextCommand command, SocketMessage message,
        ISocketMessageChannel messageChannel, MessageReference? messageReference)
    {
        var commandResponse = await command.Handle(message);
        if (commandResponse.Embed == null && string.IsNullOrEmpty(commandResponse.Description))
        {
            return commandResponse;
        }

        var messageSent = await messageChannel.SendMessageAsync(text: commandResponse.Description,
            embed: commandResponse.Embed,
            messageReference: messageReference);
        if (messageSent != null && commandResponse.Reactions != null)
        {
            await messageSent.AddReactionsAsync(commandResponse.Reactions);
        }

        return commandResponse;
    }

    private async Task HandleDiscordMessageLink(SocketGuildChannel channel, SocketMessage messageWithLink)
    {
        var discordLinkRegex =
            new Regex(
                @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

        var discordLinkMatches = discordLinkRegex.Matches(messageWithLink.Content);
        if (discordLinkMatches.Any())
        {
            foreach (Match match in discordLinkMatches)
            {
                var fullUrl = new Uri(match.Value);
                var urlSplitOnDot = fullUrl.Host.Split('.');
                if (!urlSplitOnDot.Contains("discord"))
                {
                    continue;
                }

                var segmentsJoined = string.Join("", fullUrl.Segments).Split("/");
                if (!segmentsJoined.Contains("channels"))
                {
                    continue;
                }

                if (segmentsJoined.Length < 5)
                {
                    continue;
                }

                SocketGuild guild;
                var guildIdFromUrl = Convert.ToUInt64(segmentsJoined[2]);
                if (channel.Guild.Id != guildIdFromUrl)
                {
                    guild = _client.GetGuild(guildIdFromUrl);
                    if (guild is null)
                    {
                        continue;
                    }
                }
                else
                {
                    guild = channel.Guild;
                }
                if (guild != channel.Guild)
                {
                    continue;
                }

                var channelWithMessage = await ((IGuild)guild).GetTextChannelAsync(Convert.ToUInt64(segmentsJoined[3]))
                    .ConfigureAwait(false);
                if (channelWithMessage == null)
                {
                    continue;
                }

                var messageBeingLinked = await channelWithMessage.GetMessageAsync(Convert.ToUInt64(segmentsJoined[4])).ConfigureAwait(false);
                if (messageBeingLinked is null)
                {
                    continue;
                }

                var embedBuilder = _logMessageBuilder.CreateEmbedBuilder("Message Linked", $"[Original Message]({messageBeingLinked.GetJumpUrl()}) by {messageBeingLinked.Author.Mention}:", messageBeingLinked);
                await messageWithLink.Channel.SendMessageAsync(embed: embedBuilder.Build());
            }
        }
    }

    private async Task HandleReactions(IMessage message, GuildReplyDefinition guildReplyDefinition)
    {
        if (guildReplyDefinition.Reactions != null)
        {
            foreach (var triggerResponseReaction in guildReplyDefinition.Reactions)
            {
                try
                {
                    await message.AddReactionAsync(new Emoji(triggerResponseReaction));
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, $"Failed to add reaction {triggerResponseReaction}", ex);
                }
            }
        }
    }

    private bool IsBotMentioned(SocketMessage message, IReadOnlyCollection<IGuildUser>? guildUsers)
    {
        var isBotMentioned = false;

        var isDm = message.Channel is SocketDMChannel;

        if (guildUsers != null)
        {
            var botUserInGuild = (message.Author as SocketGuildUser)?.Guild.CurrentUser;
            if (botUserInGuild != null)
            {
                isBotMentioned = _replyBusinessLayer.IsBotNameMentioned(message, botUserInGuild.Id, guildUsers);
            }
        }
        else if (isDm)
        {
            isBotMentioned = true;
        }

        return isBotMentioned;
    }

    private async Task<bool> HandleDelete(IDeletable message, string? reply)
    {
        if (string.IsNullOrEmpty(reply))
        {
            return false;
        }

        var wasDeleted = false;
        if (!reply.Contains(KeywordHandler.BuildKeyword(TriggerKeyword.DeleteMessage)))
        {
            return wasDeleted;
        }
        try
        {
            await message.DeleteAsync(new RequestOptions
            {
                AuditLogReason = "Deleted by replybot."
            });
            wasDeleted = true;
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, $"Failed to delete message: {ex.Message}", ex);
        }

        return wasDeleted;
    }

    private static string? ChooseReply(GuildReplyDefinition guildReplyDefinition, SocketUser author)
    {
        if (guildReplyDefinition.Replies == null || !guildReplyDefinition.Replies.Any())
        {
            return null;
        }

        var random = new Random();
        var randomNumber = random.Next(guildReplyDefinition.Replies.Length);
        return guildReplyDefinition.Replies[randomNumber];
    }
}