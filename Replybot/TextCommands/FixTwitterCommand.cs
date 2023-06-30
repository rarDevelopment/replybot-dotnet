using System.Text.RegularExpressions;
using Replybot.Models;

namespace Replybot.TextCommands;

public class FixTwitterCommand
{
    public readonly string NoLinkMessage = "I don't think there's a Twitter link there.";
    private const string TwitterUrlRegexPattern = "https?:\\/\\/(www.)?(twitter.com|t.co)\\/[a-z0-9_]+\\/status\\/[0-9]+";
    private readonly Regex _twitterUrlRegex = new(TwitterUrlRegexPattern, RegexOptions.IgnoreCase);
    private const string FxTwitterUrlRegexPattern = "https?:\\/\\/(www.)?(vxtwitter.com)\\/[a-z0-9_]+\\/status\\/[0-9]+";
    private readonly Regex _fxTwitterUrlRegex = new(FxTwitterUrlRegexPattern, RegexOptions.IgnoreCase);
    public const string FixTweetButtonEmojiId = "1110617858892894248";
    public const string FixTweetButtonEmojiName = "fixtweet";
    private const string OriginalTwitterBaseUrl = "twitter.com";
    private const string FixedTwitterBaseUrl = "vxtwitter.com";

    public async Task<(string fixedMessage, MessageReference messageToReplyTo)?> GetFixedTwitterMessage(
        IUserMessage requestingMessage,
        TriggerKeyword keyword)
    {
        var requestingUser = requestingMessage.Author;
        IUser userWhoSent = requestingMessage.Author;

        var requesterMessageReference = new MessageReference(requestingMessage.Id);
        var noLinkFoundTuple = (NoLinkMessage, requesterMessageReference);

        var fixedLinksResult = FixLinksIfFound(requestingMessage, requestingUser, userWhoSent, noLinkFoundTuple, keyword);
        if (fixedLinksResult != noLinkFoundTuple)
        {
            return fixedLinksResult;
        }

        if (requestingMessage.Reference == null)
        {
            return noLinkFoundTuple;
        }

        var messageReferenceId = requestingMessage.Reference.MessageId.GetValueOrDefault(default);
        if (messageReferenceId == default)
        {
            return noLinkFoundTuple;
        }

        var messageReferenced = await requestingMessage.Channel.GetMessageAsync(messageReferenceId);
        return messageReferenced is null
            ? ("I couldn't read that message for some reason, sorry!", requesterMessageReference)
            : FixLinksIfFound(messageReferenced, requestingUser, messageReferenced.Author, noLinkFoundTuple, keyword);
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
        var tweetDescribeText = fixedTweets.Count == 1 ? "tweet" : "tweets";
        var isAre = fixedTweets.Count == 1 ? "is" : "are";
        var differentUserText = requestingUser.Id != userWhoSentTweets.Id
            ? $" (in {userWhoSentTweets.Mention}'s message)"
            : "";
        var authorMentionMessage = $"{requestingUser.Mention} Here {isAre} the fixed {tweetDescribeText}{differentUserText}:\n";
        return $"{authorMentionMessage}{string.Join("\n", fixedTweets)}";
    }

    private string BuildOriginalTweetsMessage(IMessage message, IUser requestingUser, IUser userWhoSentTweets)
    {
        var fixedTweets = FixFxTwitterUrls(message);
        var tweetDescribeText = fixedTweets.Count == 1 ? "tweet" : "tweets";
        var isAre = fixedTweets.Count == 1 ? "is" : "are";
        var differentUserText = requestingUser.Id != userWhoSentTweets.Id
            ? $" (in {userWhoSentTweets.Mention}'s message)"
            : "";
        var authorMentionMessage = $"{requestingUser.Mention} Here {isAre} the original {tweetDescribeText}{differentUserText}: \n";
        return $"{authorMentionMessage}{string.Join("\n", fixedTweets)}";
    }

    public bool DoesMessageContainTwitterUrl(IMessage messageReferenced)
    {
        return _twitterUrlRegex.IsMatch(messageReferenced.Content);
    }

    public bool DoesMessageContainFxTwitterUrl(IMessage messageReferenced)
    {
        return _fxTwitterUrlRegex.IsMatch(messageReferenced.Content);
    }

    private IList<string> FixTwitterUrls(IMessage messageToFix)
    {
        var urlsFromMessage = GetTwitterUrlsFromMessage(messageToFix.Content);
        return urlsFromMessage.Select(url => url.Replace(OriginalTwitterBaseUrl, FixedTwitterBaseUrl, StringComparison.InvariantCultureIgnoreCase)).ToList();
    }

    private IList<string> FixFxTwitterUrls(IMessage messageToFix)
    {
        var urlsFromMessage = GetFxTwitterUrlsFromMessage(messageToFix.Content);
        return urlsFromMessage.Select(url => url.Replace(FixedTwitterBaseUrl, OriginalTwitterBaseUrl, StringComparison.InvariantCultureIgnoreCase)).ToList();
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

    public Emote GetFixTwitterEmote()
    {
        return Emote.Parse($"<:{FixTweetButtonEmojiName}:{FixTweetButtonEmojiId}>");
    }
}