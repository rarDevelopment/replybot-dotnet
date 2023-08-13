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
    private readonly TimeSpan _matchTimeout;

    public ReplyBusinessLayer(IReplyDataLayer replyDataLayer, BotSettings botSettings)
    {
        _replyDataLayer = replyDataLayer;
        _matchTimeout = new TimeSpan(botSettings.RegexTimeoutMilliseconds);
    }

    public async Task<GuildReplyDefinition?> GetReplyDefinition(string message,
        string? guildId,
        string? channelId = null,
        string? userId = null)
    {
        var defaultReplies = _replyDataLayer.GetDefaultReplies();
        var guildReplyDefinitions = guildId != null
            ? await _replyDataLayer.GetActiveRepliesForGuild(guildId)
            : null;

        var defaultReply = FindReplyFromData(defaultReplies, message);
        var guildReplyDefinition = guildReplyDefinitions != null
            ? FindReplyFromData(guildReplyDefinitions, message, channelId, userId)
            : null;

        return guildReplyDefinition ?? defaultReply;
    }

    private GuildReplyDefinition? FindReplyFromData(IList<GuildReplyDefinition>? replyData,
        string message,
        string? channelId = null,
        string? userId = null)
    {
        if (replyData == null || !replyData.Any())
        {
            return null;
        }

        var cleanedMessage = KeywordHandler.CleanMessageForTrigger(message);

        var matches = replyData.Where(r =>
            r.Triggers.FirstOrDefault(triggerTerm => GetWordMatch(triggerTerm, cleanedMessage)) != null).ToList();

        return matches.FirstOrDefault(r =>
            (r.ChannelIds == null || !r.ChannelIds.Any() || r.ChannelIds.Contains(channelId)) &&
            (r.UserIds == null || !r.UserIds.Any() || r.UserIds.Contains(userId)));
    }

    public bool GetWordMatch(string triggerTerm, string input)
    {
        if (triggerTerm == KeywordHandler.BuildKeyword(TriggerKeyword.Anything))
        {
            return true;
        }

        var trigger = triggerTerm.ToLower(CultureInfo.InvariantCulture).Trim();
        trigger = KeywordHandler.EscapeRegExp(trigger, _matchTimeout);
        var pattern = $"(^|(?<!\\w)){trigger}(\\b|(?!\\w))";
        return Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase, _matchTimeout);
    }

    public bool IsBotNameMentioned(SocketMessage message, ulong botUserId, IReadOnlyCollection<IGuildUser>? guildUsers)
    {
        if (guildUsers == null)
        {
            return true; // if not in a guild, this should be a DM directly to the bot, so return true
        }

        var botUser = guildUsers.First(x => x.Id == botUserId);
        var botNickname = botUser.Nickname;
        var botNameInMessage = KeywordHandler.GetBotNameInMessage(message.Content, botNickname);
        return message.MentionedUsers.Any(u => u.Id == botUserId) || !string.IsNullOrEmpty(botNameInMessage);
    }

    public string? ChooseReply(string[]? replies)
    {
        if (replies == null || !replies.Any())
        {
            return null;
        }

        var random = new Random();
        var randomNumber = random.Next(replies.Length);
        return replies[randomNumber];
    }
}