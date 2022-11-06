using Replybot.BusinessLayer;
using Replybot.Commands;
using Replybot.Models;

namespace Replybot.Events
{
    public class MessageReceivedEventHandler
    {
        private readonly IResponseBusinessLayer _responseBusinessLayer;
        private readonly KeywordHandler _keywordHandler;
        private readonly HowLongToBeatCommand _howLongToBeatCommand;
        private readonly VersionSettings _versionSettings;
        private readonly ILogger<DiscordBot> _logger;

        public MessageReceivedEventHandler(IResponseBusinessLayer responseBusinessLayer,
            KeywordHandler keywordHandler,
            HowLongToBeatCommand howLongToBeatCommand,
            VersionSettings versionSettings,
            ILogger<DiscordBot> logger)
        {
            _responseBusinessLayer = responseBusinessLayer;
            _keywordHandler = keywordHandler;
            _howLongToBeatCommand = howLongToBeatCommand;
            _versionSettings = versionSettings;
            _logger = logger;
        }

        public async Task HandleEvent(SocketMessage message)
        {
            if (!message.Author.IsBot)
            {
                var channel = message.Channel as IGuildChannel;
                var triggerResponse = await _responseBusinessLayer.GetTriggerResponse(message.Content, channel);
                if (triggerResponse == null)
                {
                    return;
                }

                var isBotMentioned = await IsBotMentioned(message, channel);
                if (triggerResponse.RequiresBotName && !isBotMentioned)
                {
                    return;
                }

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

                if (triggerResponse.Responses != null && triggerResponse.Responses.Any()
                    || triggerResponse.PeopleResponses != null && triggerResponse.PeopleResponses.Any())
                {
                    var response = ChooseResponse(triggerResponse, message.Author);

                    if (!string.IsNullOrEmpty(response))
                    {
                        var wasDeleted = await HandleDelete(message, response);
                        var messageReference = wasDeleted ? null : new MessageReference(message.Id);

                        // handle commands
                        if (response == _keywordHandler.BuildKeyword(TriggerKeyword.HowLongToBeat))
                        {
                            var howLongToBeatEmbed = await _howLongToBeatCommand.ExecuteHowLongToBeatCommand(message);
                            if (howLongToBeatEmbed != null)
                            {
                                await message.Channel.SendMessageAsync(embed: howLongToBeatEmbed, messageReference: messageReference);
                                return;
                            }
                        }

                        var messageText = _keywordHandler.ReplaceKeywords(response,
                            message.Author.Username,
                            message.Author.Id,
                            _versionSettings.VersionNumber,
                            message.Content,
                            triggerResponse,
                            message.MentionedUsers.ToList(),
                            channel?.Guild);

                        await message.Channel.SendMessageAsync(
                            messageText,
                            messageReference: messageReference
                        );
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

        private async Task<bool> HandleDelete(SocketMessage message, string response)
        {
            var wasDeleted = false;
            if (response.Contains(_keywordHandler.BuildKeyword(TriggerKeyword.DeleteMessage)))
            {
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
            }

            return wasDeleted;
        }

        private string? ChooseResponse(TriggerResponse triggerResponse, SocketUser author)
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
}
