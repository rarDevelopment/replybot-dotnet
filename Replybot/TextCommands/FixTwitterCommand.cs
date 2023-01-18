using System.Text.RegularExpressions;
using Replybot.Models;

namespace Replybot.TextCommands;

public class FixTwitterCommand
{
    private const string NoLinkMessage = "I don't think there's a Twitter link there.";
    private const string TwitterUrlRegexPattern = "https?:\\/\\/(www.)?(twitter.com|t.co)\\/[a-z0-9_]+\\/status\\/[0-9]+";
    private readonly Regex _twitterUrlRegex = new(TwitterUrlRegexPattern, RegexOptions.IgnoreCase);
    private const string FxTwitterUrlRegexPattern = "https?:\\/\\/(www.)?(fxtwitter.com)\\/[a-z0-9_]+\\/status\\/[0-9]+";
    private readonly Regex _fxTwitterUrlRegex = new(FxTwitterUrlRegexPattern, RegexOptions.IgnoreCase);

    public async Task<(string fixedTwitterMessage, MessageReference messageToReplyTo)?> GetFixedTwitterMessage(
        SocketMessage requestingMessage,
        TriggerKeyword keyword)
    {
        var requestingUser = requestingMessage.Author;
        IUser userWhoSentTweets = requestingMessage.Author;

        var requesterMessageReference = new MessageReference(requestingMessage.Id);
        var noLinkFoundTuple = (NoLinkMessage, requesterMessageReference);

        if (requestingMessage.Reference == null)
        {
            return FixLinksIfFound(requestingMessage, requestingUser, userWhoSentTweets, noLinkFoundTuple, keyword);
        }

        var messageReferenceId = requestingMessage.Reference.MessageId.GetValueOrDefault(default);
        if (messageReferenceId == default)
        {
            return
                FixLinksIfFound(requestingMessage, requestingUser, userWhoSentTweets,
                    noLinkFoundTuple, keyword);
        }

        var messageReferenced = await requestingMessage.Channel.GetMessageAsync(messageReferenceId);
        if (messageReferenced is not { } referencedSocketMessage)
        {
            return ("I couldn't read that message for some reason, sorry!", requesterMessageReference);
        }

        userWhoSentTweets = referencedSocketMessage.Author;

        return FixLinksIfFound(referencedSocketMessage, requestingUser, userWhoSentTweets, noLinkFoundTuple, keyword);
    }

    private (string, MessageReference) FixLinksIfFound(IMessage messageToFix,
        IUser requestingUser,
        IUser userWhoSentTweets,
        (string NoLinkMessage, MessageReference) noLinkFoundTuple, TriggerKeyword triggerKeyword)
    {
        return triggerKeyword switch
        {
            TriggerKeyword.FixTwitter => DoesMessageContainTwitterUrl(messageToFix)
                ? (BuildFixedTweetsMessage(messageToFix, requestingUser, userWhoSentTweets),
                    new MessageReference(messageToFix.Id))
                : noLinkFoundTuple,
            TriggerKeyword.BreakTwitter => DoesMessageContainFxTwitterUrl(messageToFix)
                ? (BuildOriginalTweetsMessage(messageToFix, requestingUser, userWhoSentTweets),
                    new MessageReference(messageToFix.Id))
                : noLinkFoundTuple,
            _ => (noLinkFoundTuple)
        };
    }

    private string BuildFixedTweetsMessage(IMessage message, IUser requestingUser, IUser userWhoSentTweets)
    {
        var fixedTweets = FixTwitterUrls(message);
        var tweetDescribeText = fixedTweets.Count == 1 ? "this tweet" : "these tweets";
        var differentUserText = requestingUser.Id != userWhoSentTweets.Id
            ? $" (in {userWhoSentTweets.Mention}'s message)"
            : "";
        var authorMentionMessage = $"{requestingUser.Mention} asked me to fix {tweetDescribeText}{differentUserText}:\n";
        return $"{authorMentionMessage}{string.Join("\n", fixedTweets)}";
    }

    private string BuildOriginalTweetsMessage(IMessage message, IUser requestingUser, IUser userWhoSentTweets)
    {
        var fixedTweets = FixFxTwitterUrls(message);
        var tweetDescribeText = fixedTweets.Count == 1 ? "tweet" : "tweets";
        var differentUserText = requestingUser.Id != userWhoSentTweets.Id
            ? $" (in {userWhoSentTweets.Mention}'s message)"
            : "";
        var authorMentionMessage = $"{requestingUser.Mention} asked for the original {tweetDescribeText}{differentUserText}:\n";
        return $"{authorMentionMessage}{string.Join("\n", fixedTweets)}";
    }

    private bool DoesMessageContainTwitterUrl(IMessage messageReferenced)
    {
        return _twitterUrlRegex.IsMatch(messageReferenced.Content);
    }

    private bool DoesMessageContainFxTwitterUrl(IMessage messageReferenced)
    {
        return _fxTwitterUrlRegex.IsMatch(messageReferenced.Content);
    }

    private IList<string> FixTwitterUrls(IMessage messageToFix)
    {
        var urlsFromMessage = GetTwitterUrlsFromMessage(messageToFix.Content);
        return urlsFromMessage.Select(url => url.Replace("twitter.com", "fxtwitter.com", StringComparison.InvariantCultureIgnoreCase)).ToList();
    }

    private IList<string> FixFxTwitterUrls(IMessage messageToFix)
    {
        var urlsFromMessage = GetFxTwitterUrlsFromMessage(messageToFix.Content);
        return urlsFromMessage.Select(url => url.Replace("fxtwitter.com", "twitter.com", StringComparison.InvariantCultureIgnoreCase)).ToList();
    }

    private IEnumerable<string> GetTwitterUrlsFromMessage(string text)
    {
        var matches = _twitterUrlRegex.Matches(text);
        return matches.Select(t => t.Value).ToList();
    }

    private IEnumerable<string> GetFxTwitterUrlsFromMessage(string text)
    {
        var matches = _fxTwitterUrlRegex.Matches(text);
        return matches.Select(t => t.Value).ToList();
    }
}