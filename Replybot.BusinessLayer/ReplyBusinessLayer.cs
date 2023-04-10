using System.Globalization;
using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Replybot.DataLayer;
using Replybot.Models;

namespace Replybot.BusinessLayer;

public class ReplyBusinessLayer : IReplyBusinessLayer
{
    private readonly IReplyDataLayer _replyDataLayer;
    private readonly KeywordHandler _keywordHandler;

    public ReplyBusinessLayer(IReplyDataLayer replyDataLayer, KeywordHandler keywordHandler)
    {
        _replyDataLayer = replyDataLayer;
        _keywordHandler = keywordHandler;
    }

    public async Task<GuildReplyDefinition?> GetReplyDefinition(string message, string? guildId)
    {
        var defaultReplies = _replyDataLayer.GetDefaultReplies();
        var guildReplyDefinitions = guildId != null
            ? await _replyDataLayer.GetRepliesForGuild(guildId)
            : null;

        var defaultReply = FindReplyFromData(defaultReplies, message);
        var guildReplyDefinition = guildReplyDefinitions != null
            ? FindReplyFromData(guildReplyDefinitions, message)
            : null;

        return guildReplyDefinition ?? defaultReply;
    }

    private GuildReplyDefinition? FindReplyFromData(IList<GuildReplyDefinition>? replyData, string message)
    {
        if (replyData == null || !replyData.Any())
        {
            return null;
        }

        var cleanedMessage = _keywordHandler.CleanMessageForTrigger(message);

        return replyData.FirstOrDefault(r =>
            r.Triggers.FirstOrDefault(triggerTerm => GetWordMatch(triggerTerm, cleanedMessage)) != null);
    }

    private bool GetWordMatch(string triggerTerm, string input)
    {
        if (triggerTerm == _keywordHandler.BuildKeyword(TriggerKeyword.Anything))
        {
            return true;
        }

        var trigger = triggerTerm.ToLower(CultureInfo.InvariantCulture).Trim();
        trigger = _keywordHandler.EscapeRegExp(trigger);
        var pattern = $"(^|(?<!\\w)){trigger}(\\b|(?!\\w))";
        var regex = new Regex(pattern);
        return regex.IsMatch(input.ToLower());
    }

    public async Task<bool> IsBotNameMentioned(SocketMessage message, IGuild? guild, ulong botUserId)
    {
        if (guild == null)
        {
            return true; // if not in a guild, this should be a DM directly to the bot, so return true
        }
        var guildUsers = await guild.GetUsersAsync();
        var botUser = guildUsers.First(x => x.Id == botUserId);
        var botNickname = botUser.Nickname;
        var botNameInMessage = _keywordHandler.GetBotNameInMessage(message.Content, botNickname);
        return message.MentionedUsers.Any(u => u.Id == botUserId) || !string.IsNullOrEmpty(botNameInMessage);
    }
}