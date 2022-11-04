using Replybot.BusinessLayer;
using Replybot.Models;

namespace Replybot.Events
{
    public class MessageReceivedEventHandler
    {
        private readonly IResponseBusinessLayer _responseBusinessLayer;
        private readonly KeywordHandler _keywordHandler;
        private readonly ILogger<DiscordBot> _logger;

        private const ulong AllowedChannelId = 123; //TODO: set these
        private const ulong BotId = 123;

        public MessageReceivedEventHandler(IResponseBusinessLayer responseBusinessLayer,
            KeywordHandler keywordHandler,
            ILogger<DiscordBot> logger)
        {
            _responseBusinessLayer = responseBusinessLayer;
            _keywordHandler = keywordHandler;
            _logger = logger;
        }

        public async Task HandleEvent(SocketMessage message)
        {
            if (!message.Author.IsBot)
            {
                if(message.Channel.Id == AllowedChannelId)
                {
                    var channel = (message.Channel as IGuildChannel)!;
                    var triggerResponse = await _responseBusinessLayer.GetTriggerResponse(message.Content, channel.GuildId);
                    if (triggerResponse == null)
                    {
                        return;
                    }

                    var isBotMentioned = await _responseBusinessLayer.IsBotNameMentioned(message, channel.Guild, BotId);

                    if (triggerResponse.RequiresBotName)
                    {
                        if (!isBotMentioned)
                        {
                            return;
                        }
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

                        var wasDeleted = false;
                        if (!string.IsNullOrEmpty(response))
                        {
                            if (response.Contains(_keywordHandler.BuildKeyword(TriggerKeyword.DeleteMessage)))
                            {
                                await message.DeleteAsync(new RequestOptions
                                {
                                    AuditLogReason = "Deleted by replybot."
                                });
                                wasDeleted = true;
                            }

                            // TODO: handle commands here

                            var messageText = _keywordHandler.ReplaceKeywords(response,
                                message.Author.Username,
                                message.Author.Id,
                                "0.0.0",
                                message.Content,
                                triggerResponse,
                                message.MentionedUsers.ToList(),
                                channel.Guild);

                            await message.Channel.SendMessageAsync(
                                messageText,
                                messageReference: wasDeleted ? null : new MessageReference(message.Id)
                            );
                        }
                    }
                }
            }
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
