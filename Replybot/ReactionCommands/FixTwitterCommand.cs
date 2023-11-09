using System.Text.RegularExpressions;
using Replybot.Models;

namespace Replybot.ReactionCommands;

public class FixTwitterCommand : IReactionCommand
{
    public readonly string NoLinkMessage = "I don't think there's a Twitter link there.";
    private const string MatchedDomainKey = "matchedDomain";
    private const string TwitterUrlRegexPattern = $"https?:\\/\\/(www.)?(?<{MatchedDomainKey}>(twitter.com|t.co|x.com|nitter.net))\\/[a-z0-9_]+\\/status\\/[0-9]+";
    private const string VxTwitterUrlRegexPattern = $"https?:\\/\\/(www.)?(?<{MatchedDomainKey}>(vxtwitter.com|fxtwitter.com|fixvx.com))\\/[a-z0-9_]+\\/status\\/[0-9]+";
    public const string FixTweetButtonEmojiId = "1110617858892894248";
    public const string FixTweetButtonEmojiName = "fixtweet";
    private const string OriginalTwitterBaseUrl = "x.com";
    private const string FixedTwitterBaseUrl = "vxtwitter.com";
    private readonly TimeSpan _matchTimeout;

    public FixTwitterCommand(BotSettings botSettings)
    {
        _matchTimeout = TimeSpan.FromMilliseconds(botSettings.RegexTimeoutTicks);
    }

    public bool CanHandle(string message, GuildConfiguration configuration)
    {
        return configuration.EnableFixTweetReactions &&
               (DoesMessageContainTwitterUrl(message) || DoesMessageContainVxTwitterUrl(message));
    }

    public Task<List<Emote>> HandleReaction(SocketMessage message)
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

    public Task<List<CommandResponse>> HandleMessage(IUserMessage message, IUser reactingUser)
    {
        string? fixedMessage;
        if (DoesMessageContainTwitterUrl(message.Content))
        {
            fixedMessage = BuildFixedTweetsMessage(message, reactingUser, message.Author);
        }
        else if (DoesMessageContainVxTwitterUrl(message.Content))
        {
            fixedMessage = BuildOriginalTweetsMessage(message, reactingUser, message.Author);
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

    private string BuildFixedTweetsMessage(IMessage message, IUser requestingUser, IUser userWhoSentTweets)
    {
        var fixedTweets = FixTwitterUrls(message);
        var tweetDescribeText = fixedTweets.Count == 1 ? "tweet" : "tweets";
        var isAre = fixedTweets.Count == 1 ? "is" : "are";
        var differentUserText = requestingUser.Id != userWhoSentTweets.Id
            ? $" (in {userWhoSentTweets.Username}'s message)"
            : "";
        var authorMentionMessage = $"{requestingUser.Mention} Here {isAre} the fixed {tweetDescribeText}{differentUserText}:\n";
        return $"{authorMentionMessage}{string.Join("\n", fixedTweets)}";
    }

    private string BuildOriginalTweetsMessage(IMessage message, IUser requestingUser, IUser userWhoSentTweets)
    {
        var fixedTweets = FixVxTwitterUrls(message);
        var tweetDescribeText = fixedTweets.Count == 1 ? "tweet" : "tweets";
        var isAre = fixedTweets.Count == 1 ? "is" : "are";
        var differentUserText = requestingUser.Id != userWhoSentTweets.Id
            ? $" (in {userWhoSentTweets.Username}'s message)"
            : "";
        var authorMentionMessage = $"{requestingUser.Mention} Here {isAre} the original {tweetDescribeText}{differentUserText}: \n";
        return $"{authorMentionMessage}{string.Join("\n", fixedTweets)}";
    }

    private bool DoesMessageContainTwitterUrl(string message)
    {
        return Regex.IsMatch(message, TwitterUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
    }

    private bool DoesMessageContainVxTwitterUrl(string message)
    {
        return Regex.IsMatch(message, VxTwitterUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
    }

    private IList<string> FixTwitterUrls(IMessage messageToFix)
    {
        var urlsFromMessage = GetTwitterUrlsFromMessage(messageToFix.Content);
        return urlsFromMessage.Select(url =>
        {
            var match = Regex.Match(url, TwitterUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
            var originalUrl = match.Groups[MatchedDomainKey].Value;
            return url.Replace(originalUrl, FixedTwitterBaseUrl,
                StringComparison.InvariantCultureIgnoreCase);
        }).ToList();
    }

    private IList<string> FixVxTwitterUrls(IMessage messageToFix)
    {
        var urlsFromMessage = GetVxTwitterUrlsFromMessage(messageToFix.Content);
        return urlsFromMessage.Select(url =>
        {
            var match = Regex.Match(url, VxTwitterUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
            var fixedUrl = match.Groups[MatchedDomainKey].Value;
            return url.Replace(fixedUrl, OriginalTwitterBaseUrl,
                StringComparison.InvariantCultureIgnoreCase);
        }).ToList();
    }

    private IEnumerable<string> GetTwitterUrlsFromMessage(string text)
    {
        var matches = Regex.Matches(text, TwitterUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        return matches.Select(t => t.Value).ToList();
    }

    private IEnumerable<string> GetVxTwitterUrlsFromMessage(string text)
    {
        var matches = Regex.Matches(text, VxTwitterUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        return matches.Select(t => t.Value).ToList();
    }

    private static Emote GetFixTwitterEmote()
    {
        return Emote.Parse($"<:{FixTweetButtonEmojiName}:{FixTweetButtonEmojiId}>");
    }
}
