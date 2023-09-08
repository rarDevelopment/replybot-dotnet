using MediatR;
using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.Notifications;
using System.Text.RegularExpressions;
using DiscordDotNetUtilities.Interfaces;
using Replybot.ReactionCommands;
using Replybot.ServiceLayer;
using Replybot.TextCommands.Models;
using static System.Text.RegularExpressions.Regex;

namespace Replybot.NotificationHandlers;
public class MessageReceivedNotificationHandler : INotificationHandler<MessageReceivedNotification>
{
    private readonly IReplyBusinessLayer _replyBusinessLayer;
    private readonly IGuildConfigurationBusinessLayer _guildConfigurationBusinessLayer;
    private readonly IEnumerable<ITextCommand> _textCommands;
    private readonly IEnumerable<IReactionCommand> _reactionCommands;
    private readonly VersionSettings _versionSettings;
    private readonly DiscordSettings _discordSettings;
    private readonly DiscordSocketClient _client;
    private readonly ExistingMessageEmbedBuilder _logMessageBuilder;
    private readonly SiteIgnoreService _siteIgnoreService;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly ILogger<DiscordBot> _logger;
    private readonly TimeSpan _matchTimeout;
    private const string PreviouslyPostedEmoji = "<:slowpoke:1149574363599868004>";

    public MessageReceivedNotificationHandler(IReplyBusinessLayer replyBusinessLayer,
        IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer,
        IEnumerable<ITextCommand> textCommands,
        IEnumerable<IReactionCommand> reactionCommands,
        BotSettings botSettings,
        VersionSettings versionSettings,
        DiscordSettings discordSettings,
        DiscordSocketClient client,
        ExistingMessageEmbedBuilder logMessageBuilder,
        SiteIgnoreService siteIgnoreService,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    {
        _replyBusinessLayer = replyBusinessLayer;
        _guildConfigurationBusinessLayer = guildConfigurationBusinessLayer;
        _textCommands = textCommands;
        _reactionCommands = reactionCommands;
        _versionSettings = versionSettings;
        _discordSettings = discordSettings;
        _client = client;
        _logMessageBuilder = logMessageBuilder;
        _siteIgnoreService = siteIgnoreService;
        _discordFormatter = discordFormatter;
        _logger = logger;
        _matchTimeout = new TimeSpan(botSettings.RegexTimeoutTicks);
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
                await HandleLinks(guildChannel, notification.Message);
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

            var reply = _replyBusinessLayer.ChooseReply(replyDefinition.Replies);

            var wasDeleted = await HandleDelete(message, reply);
            var messageReference = wasDeleted ? null : new MessageReference(message.Id);

            var defaultRepliesEnabled = config?.EnableDefaultReplies ?? true;
            if (!defaultRepliesEnabled && replyDefinition is { IsDefaultReply: true })
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
                replyDefinition);

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

        var allowedMentions = new AllowedMentions
        {
            AllowedTypes = AllowedMentionTypes.Users | AllowedMentionTypes.Roles | AllowedMentionTypes.Everyone,
            MentionRepliedUser = commandResponse.NotifyWhenReplying
        };

        var messageSent = await messageChannel.SendMessageAsync(text: commandResponse.Description,
            embed: commandResponse.Embed,
            messageReference: messageReference,
            allowedMentions: allowedMentions);
        if (messageSent != null && commandResponse.Reactions != null)
        {
            await messageSent.AddReactionsAsync(commandResponse.Reactions);
        }

        return commandResponse;
    }

    private async Task HandleLinks(SocketGuildChannel channel, SocketMessage messageWithLinks)
    {
        const string pattern = @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&//=]*)";

        var linkMatches = Matches(messageWithLinks.Content, pattern, RegexOptions.IgnoreCase, _matchTimeout);
        if (linkMatches.Any())
        {
            var links = linkMatches.Select(lm => lm.Value).ToList();
            var discordLinks = links.Where(l => l.Contains(_discordSettings.BaseUrl));
            var otherLinks = links.Where(l => !l.Contains(_discordSettings.BaseUrl));

            await HandleDiscordMessageLinks(channel, messageWithLinks, discordLinks);

            await HandleGeneralLinks(messageWithLinks, otherLinks);
        }
    }

    private async Task HandleGeneralLinks(SocketMessage messageWithLinks, IEnumerable<string> otherLinks)
    {
        if (messageWithLinks.Channel is not SocketTextChannel textChannel)
        {
            return;
        }

        var messages = (await textChannel.GetMessagesAsync(messageWithLinks, Direction.Before, 150).FlattenAsync()).ToList();

        if (!messages.Any())
        {
            return;
        }

        var sitesToIgnore = await _siteIgnoreService.GetSiteIgnoreList();
        var sitesToIgnoreList = sitesToIgnore?.Split("\n").Where(s => s.Trim().Length > 0).ToList();

        foreach (var link in otherLinks)
        {
            if (ShouldIgnoreLink(sitesToIgnoreList, link))
            {
                continue;
            }
            var relevantMessage =
                messages.OrderByDescending(m => m.Timestamp).FirstOrDefault(m => m.Content.Contains(link, StringComparison.InvariantCultureIgnoreCase));
            if (relevantMessage == null)
            {
                continue;
            }

            var embed = _discordFormatter.BuildRegularEmbed("Link Posted Previously",
                $"{PreviouslyPostedEmoji} This link was posted earlier by {relevantMessage.Author.Mention}! {PreviouslyPostedEmoji}\n[Click here to go to the previous discussion]({relevantMessage.GetJumpUrl()}).");
            await messageWithLinks.Channel.SendMessageAsync(embed: embed, messageReference: new MessageReference(messageWithLinks.Id));
        }
    }

    private static bool ShouldIgnoreLink(IReadOnlyCollection<string>? sitesToIgnore, string link)
    {
        return sitesToIgnore != null &&
               sitesToIgnore.Any(s => link.Contains(s,
                   StringComparison.InvariantCultureIgnoreCase));
    }

    private async Task HandleDiscordMessageLinks(SocketGuildChannel channel, SocketMessage messageWithLinks,
        IEnumerable<string> discordLinks)
    {
        foreach (var link in discordLinks)
        {
            var fullUrl = new Uri(link);
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

            var messageBeingLinked = await channelWithMessage.GetMessageAsync(Convert.ToUInt64(segmentsJoined[4]))
                .ConfigureAwait(false);
            if (messageBeingLinked is null)
            {
                continue;
            }

            var embedBuilder = _logMessageBuilder.CreateEmbedBuilder("Message Linked",
                $"[Original Message]({messageBeingLinked.GetJumpUrl()}) by {messageBeingLinked.Author.Mention}:",
                messageBeingLinked);
            await messageWithLinks.Channel.SendMessageAsync(embed: embedBuilder.Build());
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
}