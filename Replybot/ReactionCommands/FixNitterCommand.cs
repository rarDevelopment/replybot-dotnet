using System.Text.RegularExpressions;
using DiscordDotNetUtilities.Interfaces;
using Replybot.Models;

namespace Replybot.ReactionCommands;

public class FixNitterCommand : IReactionCommand
{
    private readonly IDiscordFormatter _discordFormatter;
    public readonly string NoLinkMessage = "I don't think there's a relevant link there.";
    private const string MatchedDomainKey = "matchedDomain";
    private const string TwitterUrlRegexPattern = $"https?:\\/\\/(www.)?(?<{MatchedDomainKey}>(twitter.com|t.co|x.com))\\/[a-z0-9_]+\\/status\\/[0-9]+";
    private const string NitterUrlRegexPattern = "https?:\\/\\/(www.)?(nitter.net)\\/[a-z0-9_]+\\/status\\/[0-9]+";
    public const string FixTweetButtonEmojiId = "1133174470966784100";
    public const string FixNitterButtonEmojiName = "fixnitter";
    private const string OriginalTwitterBaseUrl = "twitter.com";
    private const string NitterBaseUrl = "nitter.net";
    private readonly TimeSpan _matchTimeout;

    public FixNitterCommand(IDiscordFormatter discordFormatter, BotSettings botSettings)
    {
        _discordFormatter = discordFormatter;
        _matchTimeout = TimeSpan.FromMilliseconds(botSettings.RegexTimeoutTicks);
    }

    public bool CanHandle(string message, GuildConfiguration configuration)
    {
        return configuration.EnableFixTweetReactions &&
               (DoesMessageContainTwitterUrl(message) || DoesMessageContainNitterUrl(message));
    }

    public Task<List<Emote>> HandleReaction(SocketMessage message)
    {
        var emotes = new List<Emote>
        {
            GetFixNitterEmote()
        };
        return Task.FromResult(emotes);
    }

    public bool IsReacting(IEmote reactionEmote, GuildConfiguration guildConfiguration)
    {
        return guildConfiguration.EnableFixTweetReactions && Equals(reactionEmote, GetFixNitterEmote());
    }

    public Task<List<CommandResponse>> HandleMessage(IUserMessage message, IUser reactingUser)
    {
        string? fixedMessage;
        if (DoesMessageContainTwitterUrl(message.Content))
        {
            fixedMessage = BuildNitterMessage(message, reactingUser, message.Author);
        }
        else if (DoesMessageContainNitterUrl(message.Content))
        {
            fixedMessage = BuildOriginalTweetsMessage(message, reactingUser, message.Author);
        }
        else
        {
            fixedMessage = NoLinkMessage;
        }

        var messagesToSend = new List<CommandResponse>
        {
            new() { Embed = _discordFormatter.BuildRegularEmbed("View Tweets Without An Account", fixedMessage, reactingUser) }
        };
        return Task.FromResult(messagesToSend);
    }

    private string BuildNitterMessage(IMessage message, IUser requestingUser, IUser userWhoSentTweets)
    {
        var fixedTweets = FixTwitterUrls(message);
        var tweetDescribeText = fixedTweets.Count == 1 ? "tweet" : "tweets";
        var differentUserText = requestingUser.Id != userWhoSentTweets.Id
            ? $" (in {userWhoSentTweets.Mention}'s message)"
            : "";
        var authorMentionMessage = $"{requestingUser.Mention} You can use the following to view the linked {tweetDescribeText}{differentUserText} without an account (it is correct that there's no preview):\n";
        return $"{authorMentionMessage}{string.Join("\n", fixedTweets.Select(t => $"[{t.Replace($"https://{NitterBaseUrl}", "")}]({t})"))}";
    }

    private string BuildOriginalTweetsMessage(IMessage message, IUser requestingUser, IUser userWhoSentTweets)
    {
        var fixedTweets = FixNitterUrls(message);
        var tweetDescribeText = fixedTweets.Count == 1 ? "tweet" : "tweets";
        var isAre = fixedTweets.Count == 1 ? "is" : "are";
        var differentUserText = requestingUser.Id != userWhoSentTweets.Id
            ? $" (in {userWhoSentTweets.Mention}'s message)"
            : "";
        var authorMentionMessage = $"{requestingUser.Mention} Here {isAre} the original {tweetDescribeText}{differentUserText}: \n";
        return $"{authorMentionMessage}{string.Join("\n", fixedTweets)}";
    }

    private bool DoesMessageContainTwitterUrl(string message)
    {
        return Regex.IsMatch(message, TwitterUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
    }

    private bool DoesMessageContainNitterUrl(string message)
    {
        return Regex.IsMatch(message, NitterUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
    }

    private IList<string> FixTwitterUrls(IMessage messageToFix)
    {
        var urlsFromMessage = GetTwitterUrlsFromMessage(messageToFix.Content);
        return urlsFromMessage.Select(url => url.Replace(OriginalTwitterBaseUrl, NitterBaseUrl, StringComparison.InvariantCultureIgnoreCase)).ToList();
    }

    private IList<string> FixNitterUrls(IMessage messageToFix)
    {
        var urlsFromMessage = GetNitterUrlsFromMessage(messageToFix.Content);
        return urlsFromMessage.Select(url =>
        {
            var match = Regex.Match(url, TwitterUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
            var originalUrl = match.Groups[MatchedDomainKey].Value;
            return url.Replace(originalUrl, NitterBaseUrl,
                StringComparison.InvariantCultureIgnoreCase);
        }).ToList();
    }

    private IEnumerable<string> GetTwitterUrlsFromMessage(string text)
    {
        var matches = Regex.Matches(text, TwitterUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        return matches.Select(t => t.Value).ToList();
    }

    private IEnumerable<string> GetNitterUrlsFromMessage(string text)
    {
        var matches = Regex.Matches(text, NitterUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        return matches.Select(t => t.Value).ToList();
    }

    private static Emote GetFixNitterEmote()
    {
        return Emote.Parse($"<:{FixNitterButtonEmojiName}:{FixTweetButtonEmojiId}>");
    }
}