using System.Globalization;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Replybot.DataLayer;
using Replybot.Models;

namespace Replybot.BusinessLayer
{
    public class ResponseBusinessLayer : IResponseBusinessLayer
    {
        private readonly IResponseDataLayer _responseDataLayer;
        private readonly KeywordHandler _keywordHandler;

        public ResponseBusinessLayer(IResponseDataLayer responseDataLayer, KeywordHandler keywordHandler)
        {
            _responseDataLayer = responseDataLayer;
            _keywordHandler = keywordHandler;
        }
        
        public async Task<TriggerResponse?> GetTriggerResponse(string message, ulong guildId)
        {
            var defaultResponses = _responseDataLayer.GetDefaultResponses();
            var guildResponses = await _responseDataLayer.GetResponsesForGuild(guildId);

            var defaultResponse = FindResponseFromData(defaultResponses, message);
            var guildResponse = FindResponseFromData(guildResponses, message);

            return guildResponse ?? defaultResponse;
        }

        private TriggerResponse? FindResponseFromData(IList<TriggerResponse>? responseData, string message)
        {
            if (responseData == null || !responseData.Any())
            {
                return null;
            }

            var cleanedMessage = _keywordHandler.CleanMessageForTrigger(message);
            var foundTriggerResponse = responseData.FirstOrDefault(r =>
                r.Triggers.FirstOrDefault(
                    triggerTerm => GetWordMatch(triggerTerm, cleanedMessage)) != null);
            return foundTriggerResponse;
        }

        private bool GetWordMatch(string triggerTerm, string input)
        {
            if (triggerTerm == _keywordHandler.BuildKeyword(TriggerKeyword.Anything))
            {
                return true;
            }

            var trigger = triggerTerm.ToLower(CultureInfo.InvariantCulture);
            trigger = _keywordHandler.EscapeRegExp(trigger);
            var pattern = $"(^|(?<!\\w)){trigger}(\\b|(?!\\w))";
            var regex = new Regex(pattern);
            return regex.IsMatch(input.ToLower());
        }

        public async Task<bool> IsBotNameMentioned(SocketMessage message, IGuild guild, ulong botUserId)
        {
            var guildUsers = await guild.GetUsersAsync();
            var botUser = guildUsers.First(x => x.Id == botUserId);
            var botNickname = botUser.Nickname;
            var botNameInMessage = _keywordHandler.GetBotNameInMessage(message.Content, botNickname);
            return message.MentionedUsers.Any(u => u.Id == botUserId) || !string.IsNullOrEmpty(botNameInMessage);
        }
    }

    public interface IResponseBusinessLayer
    {
        Task<TriggerResponse?> GetTriggerResponse(string message, ulong guildId);
        Task<bool> IsBotNameMentioned(SocketMessage message, IGuild guild, ulong botUserId);
    }
}
