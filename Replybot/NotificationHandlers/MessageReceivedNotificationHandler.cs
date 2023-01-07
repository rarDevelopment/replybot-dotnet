using MediatR;
using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.Notifications;
using Replybot.TextCommands;
using System.Text.RegularExpressions;

namespace Replybot.NotificationHandlers;
public class MessageReceivedNotificationHandler : INotificationHandler<MessageReceivedNotification>
{
    private readonly IResponseBusinessLayer _responseBusinessLayer;
    private readonly KeywordHandler _keywordHandler;
    private readonly HowLongToBeatCommand _howLongToBeatCommand;
    private readonly DefineWordCommand _defineWordCommand;
    private readonly GetFortniteShopInformationCommand _fortniteShopInformationCommand;
    private readonly PollCommand _pollCommand;
    private readonly FixTwitterCommand _fixTwitterCommand;
    private readonly VersionSettings _versionSettings;
    private readonly DiscordSocketClient _client;
    private readonly LogMessageBuilder _logMessageBuilder;
    private readonly ILogger<DiscordBot> _logger;

    public MessageReceivedNotificationHandler(IResponseBusinessLayer responseBusinessLayer,
        KeywordHandler keywordHandler,
        HowLongToBeatCommand howLongToBeatCommand,
        DefineWordCommand defineWordCommand,
        GetFortniteShopInformationCommand fortniteShopInformationCommand,
        PollCommand pollCommand,
        FixTwitterCommand fixTwitterCommand,
        VersionSettings versionSettings,
        DiscordSocketClient client,
        LogMessageBuilder logMessageBuilder,
        ILogger<DiscordBot> logger)
    {
        _responseBusinessLayer = responseBusinessLayer;
        _keywordHandler = keywordHandler;
        _howLongToBeatCommand = howLongToBeatCommand;
        _defineWordCommand = defineWordCommand;
        _fortniteShopInformationCommand = fortniteShopInformationCommand;
        _pollCommand = pollCommand;
        _fixTwitterCommand = fixTwitterCommand;
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

            if (message.Channel is not SocketTextChannel channel)
            {
                return Task.CompletedTask;
            }

            await HandleDiscordMessageLink(channel, notification.Message);

            var triggerResponse = await _responseBusinessLayer.GetTriggerResponse(message.Content, channel);
            if (triggerResponse == null)
            {
                return Task.CompletedTask;
            }

            var isBotMentioned = await IsBotMentioned(message, channel);
            if (triggerResponse.RequiresBotName && !isBotMentioned)
            {
                return Task.CompletedTask;
            }

            await HandleReactions(message, triggerResponse);

            if ((triggerResponse.Responses == null || !triggerResponse.Responses.Any()) &&
                (triggerResponse.PeopleResponses == null || !triggerResponse.PeopleResponses.Any()))
            {
                return Task.CompletedTask;
            }
            var response = ChooseResponse(triggerResponse, message.Author);

            if (string.IsNullOrEmpty(response))
            {
                return Task.CompletedTask;
            }
            var wasDeleted = await HandleDelete(message, response);
            var messageReference = wasDeleted ? null : new MessageReference(message.Id);

            // handle commands
            if (response == _keywordHandler.BuildKeyword(TriggerKeyword.HowLongToBeat))
            {
                var howLongToBeatEmbed = await _howLongToBeatCommand.GetHowLongToBeatEmbed(message);
                if (howLongToBeatEmbed != null)
                {
                    await message.Channel.SendMessageAsync(embed: howLongToBeatEmbed,
                        messageReference: messageReference);
                }

                return Task.CompletedTask;
            }

            if (response == _keywordHandler.BuildKeyword(TriggerKeyword.DefineWord))
            {
                var defineWordEmbed = await _defineWordCommand.GetWordDefinitionEmbed(message);
                if (defineWordEmbed != null)
                {
                    await message.Channel.SendMessageAsync(embed: defineWordEmbed,
                        messageReference: messageReference);
                }

                return Task.CompletedTask;
            }

            if (response == _keywordHandler.BuildKeyword(TriggerKeyword.FortniteShopInfo))
            {
                var fortniteShopInfoEmbed =
                    await _fortniteShopInformationCommand.GetFortniteShopInformationEmbed(message);
                if (fortniteShopInfoEmbed != null)
                {
                    await message.Channel.SendMessageAsync(embed: fortniteShopInfoEmbed,
                        messageReference: messageReference);
                }

                return Task.CompletedTask;
            }

            if (response == _keywordHandler.BuildKeyword(TriggerKeyword.Poll))
            {
                var (pollEmbed, reactionEmotes) = _pollCommand.BuildPollEmbed(message);
                if (pollEmbed == null)
                {
                    return Task.CompletedTask;
                }
                var messageSent = await message.Channel.SendMessageAsync(embed: pollEmbed,
                    messageReference: messageReference);
                if (messageSent != null && reactionEmotes != null)
                {
                    await messageSent.AddReactionsAsync(reactionEmotes);
                }

                return Task.CompletedTask;
            }

            if (response == _keywordHandler.BuildKeyword(TriggerKeyword.FixTwitter))
            {
                var fixedTwitterMessage = await _fixTwitterCommand.GetFixedTwitterMessage(channel, notification.Message);
                if (fixedTwitterMessage != null)
                {
                    await message.Channel.SendMessageAsync(fixedTwitterMessage.Value.fixedTwitterMessage,
                        messageReference: fixedTwitterMessage.Value.messageToReplyTo);
                    return Task.CompletedTask;
                }
            }

            var messageText = _keywordHandler.ReplaceKeywords(response,
                message.Author.Username,
                message.Author.Id,
                _versionSettings.VersionNumber,
                message.Content,
                triggerResponse,
                message.MentionedUsers.ToList(),
                channel.Guild);

            await message.Channel.SendMessageAsync(
                messageText,
                messageReference: messageReference
            );

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
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

    private async Task HandleReactions(IMessage message, TriggerResponse triggerResponse)
    {
        if (triggerResponse.Reactions != null)
        {
            foreach (var triggerResponseReaction in triggerResponse.Reactions)
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

    private async Task<bool> IsBotMentioned(SocketMessage message, IGuildChannel? channel)
    {
        var isBotMentioned = false;

        var botUserInGuild = (message.Author as SocketGuildUser)?.Guild.CurrentUser;
        var isDm = message.Channel is SocketDMChannel;

        if (botUserInGuild != null)
        {
            isBotMentioned = await _responseBusinessLayer.IsBotNameMentioned(message, channel?.Guild, botUserInGuild.Id);
        }
        else if (isDm)
        {
            isBotMentioned = true;
        }

        return isBotMentioned;
    }

    private async Task<bool> HandleDelete(IDeletable message, string response)
    {
        var wasDeleted = false;
        if (!response.Contains(_keywordHandler.BuildKeyword(TriggerKeyword.DeleteMessage)))
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

    private static string? ChooseResponse(TriggerResponse triggerResponse, SocketUser author)
    {
        var responseOptions = triggerResponse.Responses;
        if (triggerResponse.PeopleResponses != null && triggerResponse.PeopleResponses.Any())
        {
            var personId = author.Id;
            if (triggerResponse.PeopleResponses.Any(pr => pr.UserId == personId))
            {
                responseOptions = triggerResponse.PeopleResponses.First(pr => pr.UserId == personId).Responses;
            }
        }

        if (responseOptions == null || !responseOptions.Any())
        {
            return null;
        }

        var random = new Random();
        var randomNumber = random.Next(responseOptions.Length);
        return responseOptions[randomNumber];
    }
}