using System.Text.RegularExpressions;
using Replybot.Models;
using static System.Text.RegularExpressions.Regex;

namespace Replybot.ReactionCommands;

public class FixTikTokCommand(BotSettings botSettings) : IReactionCommand
{
    public readonly string NoLinkMessage = "I don't think there's a TikTok link there.";
    private const string TikTokUrlRegexPattern = "https?:\\/\\/(vm.)?(tiktok.com)/[a-z0-9-_//]+";
    private const string VxTikTokUrlRegexPattern = "https?:\\/\\/(vm.)?(vxtiktok.com)/[a-z0-9-_//]+";
    private readonly TimeSpan _matchTimeout = new(botSettings.RegexTimeoutTicks);
    public const string FixTikTokButtonEmojiId = "1177645488858742844";
    public const string FixTikTokButtonEmojiName = "fixtiktok";
    private const string OriginalTikTokBaseUrl = "tiktok.com";
    private const string FixedTikTokBaseUrl = "vxtiktok.com";

    public bool CanHandle(string message, GuildConfiguration configuration)
    {
        return configuration.EnableFixTikTokReactions &&
               (DoesMessageContainTikTokUrl(message) || DoesMessageContainVxTikTokUrl(message));
    }

    public Task<List<Emote>> HandleReaction(SocketMessage message)
    {
        var emotes = new List<Emote>
        {
            GetFixTikTokEmote()
        };
        return Task.FromResult(emotes);
    }

    public bool IsReacting(IEmote reactionEmote, GuildConfiguration guildConfiguration)
    {
        return guildConfiguration.EnableFixTikTokReactions && Equals(reactionEmote, GetFixTikTokEmote());
    }

    public Task<List<CommandResponse>> HandleMessage(IUserMessage message, IUser reactingUser)
    {
        string? fixedMessage;
        if (DoesMessageContainTikTokUrl(message.Content))
        {
            fixedMessage = BuildFixedTikTokMessage(message, reactingUser, message.Author);
        }
        else if (DoesMessageContainVxTikTokUrl(message.Content))
        {
            fixedMessage = BuildOriginalTikTokMessage(message, reactingUser, message.Author);
        }
        else
        {
            fixedMessage = NoLinkMessage;
        }

        var messagesToSend = new List<CommandResponse>
        {
            new() { Description = fixedMessage, NotifyWhenReplying = false, AllowDeleteButton = true }
        };
        return Task.FromResult(messagesToSend);
    }

    private string BuildFixedTikTokMessage(IMessage message, IUser requestingUser, IUser userWhoSent)
    {
        var fixedTikTokUrls = FixTikTokUrls(message);
        var describeText = fixedTikTokUrls.Count == 1 ? "post" : "posts";
        var isAre = fixedTikTokUrls.Count == 1 ? "is" : "are";
        var differentUserText = requestingUser.Id != userWhoSent.Id
            ? $" (in {userWhoSent.Username}'s message)"
            : "";
        var authorMentionMessage = $"{requestingUser.Mention} Here {isAre} the fixed TikTok {describeText}{differentUserText}:\n";
        return $"{authorMentionMessage}{string.Join("\n", fixedTikTokUrls)}";
    }

    private string BuildOriginalTikTokMessage(IMessage message, IUser requestingUser, IUser userWhoSent)
    {
        var fixedTikTokUrls = FixVxTikTokUrls(message);
        var describeText = fixedTikTokUrls.Count == 1 ? "post" : "posts";
        var isAre = fixedTikTokUrls.Count == 1 ? "is" : "are";
        var differentUserText = requestingUser.Id != userWhoSent.Id
            ? $" (in {userWhoSent.Username}'s message)"
            : "";
        var authorMentionMessage = $"{requestingUser.Mention} Here {isAre} the original {describeText}{differentUserText}: \n";
        return $"{authorMentionMessage}{string.Join("\n", fixedTikTokUrls)}";
    }

    private bool DoesMessageContainTikTokUrl(string message)
    {
        return IsMatch(message, TikTokUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
    }

    private bool DoesMessageContainVxTikTokUrl(string message)
    {
        return IsMatch(message, VxTikTokUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
    }

    private List<string> FixTikTokUrls(IMessage messageToFix)
    {
        var urlsFromMessage = GetTikTokUrlsFromMessage(messageToFix.Content);
        return urlsFromMessage.Select(url => url.Replace(OriginalTikTokBaseUrl, FixedTikTokBaseUrl, StringComparison.InvariantCultureIgnoreCase)).ToList();
    }

    private List<string> FixVxTikTokUrls(IMessage messageToFix)
    {
        var urlsFromMessage = GetDdTikTokUrlsFromMessage(messageToFix.Content);
        return urlsFromMessage.Select(url => url.Replace(FixedTikTokBaseUrl, OriginalTikTokBaseUrl, StringComparison.InvariantCultureIgnoreCase)).ToList();
    }

    private List<string> GetTikTokUrlsFromMessage(string text)
    {
        var matches = Matches(text, TikTokUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        return matches.Select(t => t.Value).ToList();
    }

    private List<string> GetDdTikTokUrlsFromMessage(string text)
    {
        var matches = Matches(text, VxTikTokUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        return matches.Select(t => t.Value).ToList();
    }

    private static Emote GetFixTikTokEmote()
    {
        return Emote.Parse($"<:{FixTikTokButtonEmojiName}:{FixTikTokButtonEmojiId}>");
    }
}