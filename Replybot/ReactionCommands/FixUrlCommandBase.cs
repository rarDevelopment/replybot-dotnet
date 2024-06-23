using System.Text.RegularExpressions;
using Replybot.Models;
using Replybot.TextCommands.Models;

namespace Replybot.ReactionCommands;

public abstract class FixUrlCommandBase(FixLinkConfig fixLinkConfig, BotSettings botSettings)
{
    private readonly TimeSpan _matchTimeout = TimeSpan.FromMilliseconds(botSettings.RegexTimeoutTicks);

    protected string BuildFixedUrlsMessage(IMessage message, IUser requestingUser, IUser userWhoSentUrls)
    {
        var fixedUrls = ReplaceOriginalUrls(message);
        var descriptionText = fixedUrls.Count == 1 ? "URL" : "URLs";
        var isAre = fixedUrls.Count == 1 ? "is" : "are";
        var differentUserText = requestingUser.Id != userWhoSentUrls.Id
            ? $" (in {userWhoSentUrls.Username}'s message)"
            : "";
        var authorMentionMessage = $"{requestingUser.Mention} Here {isAre} the fixed {descriptionText}{differentUserText}:\n";
        return $"{authorMentionMessage}{string.Join((string?)"\n", fixedUrls)}\n{fixLinkConfig.AdditionalMessage ?? ""}";
    }

    protected string BuildOriginalUrlsMessage(IMessage message, IUser requestingUser, IUser userWhoSentUrls)
    {
        var fixedUrls = ReplaceFixedUrls(message);
        var descriptionText = fixedUrls.Count == 1 ? "URL" : "URLs";
        var isAre = fixedUrls.Count == 1 ? "is" : "are";
        var differentUserText = requestingUser.Id != userWhoSentUrls.Id
            ? $" (in {userWhoSentUrls.Username}'s message)"
            : "";
        var authorMentionMessage = $"{requestingUser.Mention} Here {isAre} the original {descriptionText}{differentUserText}: \n";
        return $"{authorMentionMessage}{string.Join((string?)"\n", fixedUrls)}\n{fixLinkConfig.AdditionalMessage ?? ""}";
    }

    protected bool DoesMessageContainOriginalUrl(string message)
    {
        return Regex.IsMatch(message, fixLinkConfig.OriginalUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
    }

    protected bool DoesMessageContainFixedUrl(string message)
    {
        return Regex.IsMatch(message, fixLinkConfig.FixedUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
    }

    private List<string> ReplaceOriginalUrls(IMessage messageToFix)
    {
        var urlsFromMessage = GetOriginalUrlsFromMessage(messageToFix.Content);
        return urlsFromMessage.Select(url =>
        {
            var match = Regex.Match(url, fixLinkConfig.OriginalUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
            var originalUrl = !string.IsNullOrEmpty(fixLinkConfig.MatchedDomainKey)
                ? match.Groups[fixLinkConfig.MatchedDomainKey].Value
                : fixLinkConfig.OriginalBaseUrl;
            return url.Replace(originalUrl, fixLinkConfig.FixedBaseUrl,
                StringComparison.InvariantCultureIgnoreCase);
        }).ToList();
    }

    private List<string> ReplaceFixedUrls(IMessage messageToFix)
    {
        var urlsFromMessage = GetFixedUrlsFromMessage(messageToFix.Content);
        return urlsFromMessage.Select(url =>
        {
            var match = Regex.Match(url, fixLinkConfig.FixedUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
            var fixedUrl = !string.IsNullOrEmpty(fixLinkConfig.MatchedDomainKey)
                ? match.Groups[fixLinkConfig.MatchedDomainKey].Value
                : fixLinkConfig.FixedBaseUrl;
            return url.Replace(fixedUrl, fixLinkConfig.OriginalBaseUrl,
                StringComparison.InvariantCultureIgnoreCase);
        }).ToList();
    }

    private IEnumerable<string> GetOriginalUrlsFromMessage(string text)
    {
        var matches = Regex.Matches(text, fixLinkConfig.OriginalUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        return matches.Select(t => t.Value).ToList();
    }

    private IEnumerable<string> GetFixedUrlsFromMessage(string text)
    {
        var matches = Regex.Matches(text, fixLinkConfig.FixedUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        return matches.Select(t => t.Value).ToList();
    }

    protected Emote GetEmote()
    {
        return new Emote(fixLinkConfig.FixUrlButtonEmojiId, fixLinkConfig.FixUrlButtonEmojiName);
    }
}