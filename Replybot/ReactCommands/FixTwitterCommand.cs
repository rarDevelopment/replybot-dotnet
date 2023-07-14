using System.Text.RegularExpressions;
using Replybot.Models;
using Replybot.TextCommands;

namespace Replybot.ReactCommands;

public class FixTwitterCommand : IReactCommand
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

    public bool CanHandle(string message, GuildConfiguration configuration)
    {
        return configuration.EnableFixTweetReactions &&
               (DoesMessageContainTwitterUrl(message) || DoesMessageContainFxTwitterUrl(message));
    }

    public Task<List<Emote>> HandleReact(SocketMessage message)
    {
        var emotes = new List<Emote>
        {
            GetFixTwitterEmote()
        };
        return Task.FromResult(emotes);
    }

    public bool IsReacting(IEmote reactionEmote, GuildConfiguration guildConfiguration)
    {
        return guildConfiguration.EnableFixTweetReactions && Equals(reactionEmote, GetFixTwitterEmote());
    }

    public Task<List<CommandResponse>> HandleMessage(IUserMessage message)
    {
        string? fixedMessage;
        if (DoesMessageContainTwitterUrl(message.Content))
        {
            fixedMessage = BuildFixedTweetsMessage(message, message.Author, message.Author);
        }
        else if (DoesMessageContainFxTwitterUrl(message.Content))
        {
            fixedMessage = BuildOriginalTweetsMessage(message, message.Author, message.Author);
        }
        else
        {
            fixedMessage = NoLinkMessage;
        }

        var messagesToSend = new List<CommandResponse>
        {
            new() { Description = fixedMessage }
        };
        return Task.FromResult(messagesToSend);
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

    public bool DoesMessageContainTwitterUrl(string message)
    {
        return _twitterUrlRegex.IsMatch(message);
    }

    public bool DoesMessageContainFxTwitterUrl(string message)
    {
        return _fxTwitterUrlRegex.IsMatch(message);
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

    private static Emote GetFixTwitterEmote()
    {
        return Emote.Parse($"<:{FixTweetButtonEmojiName}:{FixTweetButtonEmojiId}>");
    }
}