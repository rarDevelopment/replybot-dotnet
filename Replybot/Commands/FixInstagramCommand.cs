using System.Text.RegularExpressions;
using Replybot.Models;

namespace Replybot.Commands;

public class FixInstagramCommand : IReactCommand
{
    public readonly string NoLinkMessage = "I don't think there's an Instagram link there.";
    private const string InstagramUrlRegexPattern = "https?:\\/\\/(www.)?(instagram.com)\\/(p|reel|reels)\\/[a-z0-9-_]+";
    private readonly Regex _instagramUrlRegex = new(InstagramUrlRegexPattern, RegexOptions.IgnoreCase);
    private const string DdInstagramUrlRegexPattern = "https?:\\/\\/(www.)?(ddinstagram.com)\\/(p|reel|reels)\\/[a-z0-9-_]+";
    private readonly Regex _ddInstagramUrlRegex = new(DdInstagramUrlRegexPattern, RegexOptions.IgnoreCase);
    public const string FixInstagramButtonEmojiId = "1116574189592260658";
    public const string FixInstagramButtonEmojiName = "fixinstagram";
    private const string OriginalInstagramBaseUrl = "instagram.com";
    private const string FixedInstagramBaseUrl = "ddinstagram.com";

    public bool CanHandle(string message, GuildConfiguration configuration)
    {
        return configuration.EnableFixInstagramReactions &&
               (DoesMessageContainInstagramUrl(message) || DoesMessageContainDdInstagramUrl(message));
    }

    public Task<List<Emote>> HandleReact(SocketMessage message)
    {
        var emotes = new List<Emote>
        {
            GetFixInstagramEmote()
        };
        return Task.FromResult(emotes);
    }

    public bool IsReacting(IEmote reactionEmote, GuildConfiguration guildConfiguration)
    {
        return guildConfiguration.EnableFixInstagramReactions && Equals(reactionEmote, GetFixInstagramEmote());
    }

    public Task<List<CommandResponse>> HandleMessage(IUserMessage message)
    {
        string? fixedMessage;
        if (DoesMessageContainInstagramUrl(message.Content))
        {
            fixedMessage = BuildFixedInstagramMessage(message, message.Author, message.Author);
        }
        else if (DoesMessageContainDdInstagramUrl(message.Content))
        {
            fixedMessage = BuildOriginalInstagramMessage(message, message.Author, message.Author);
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

    private string BuildFixedInstagramMessage(IMessage message, IUser requestingUser, IUser userWhoSent)
    {
        var fixedInstagramUrls = FixInstagramUrls(message);
        var describeText = fixedInstagramUrls.Count == 1 ? "post" : "posts";
        var isAre = fixedInstagramUrls.Count == 1 ? "is" : "are";
        var differentUserText = requestingUser.Id != userWhoSent.Id
            ? $" (in {userWhoSent.Mention}'s message)"
            : "";
        var authorMentionMessage = $"{requestingUser.Mention} Here {isAre} the fixed Instagram {describeText}{differentUserText}:\n";
        return $"{authorMentionMessage}{string.Join("\n", fixedInstagramUrls)}";
    }

    private string BuildOriginalInstagramMessage(IMessage message, IUser requestingUser, IUser userWhoSent)
    {
        var fixedInstagramUrls = FixDdInstagramUrls(message);
        var describeText = fixedInstagramUrls.Count == 1 ? "post" : "posts";
        var isAre = fixedInstagramUrls.Count == 1 ? "is" : "are";
        var differentUserText = requestingUser.Id != userWhoSent.Id
            ? $" (in {userWhoSent.Mention}'s message)"
            : "";
        var authorMentionMessage = $"{requestingUser.Mention} Here {isAre} the original {describeText}{differentUserText}: \n";
        return $"{authorMentionMessage}{string.Join("\n", fixedInstagramUrls)}";
    }

    public bool DoesMessageContainInstagramUrl(string message)
    {
        return _instagramUrlRegex.IsMatch(message);
    }

    public bool DoesMessageContainDdInstagramUrl(string message)
    {
        return _ddInstagramUrlRegex.IsMatch(message);
    }

    private IList<string> FixInstagramUrls(IMessage messageToFix)
    {
        var urlsFromMessage = GetInstagramUrlsFromMessage(messageToFix.Content);
        return urlsFromMessage.Select(url => url.Replace(OriginalInstagramBaseUrl, FixedInstagramBaseUrl, StringComparison.InvariantCultureIgnoreCase)).ToList();
    }

    private IList<string> FixDdInstagramUrls(IMessage messageToFix)
    {
        var urlsFromMessage = GetDdInstagramUrlsFromMessage(messageToFix.Content);
        return urlsFromMessage.Select(url => url.Replace(FixedInstagramBaseUrl, OriginalInstagramBaseUrl, StringComparison.InvariantCultureIgnoreCase)).ToList();
    }

    private IEnumerable<string> GetInstagramUrlsFromMessage(string text)
    {
        var matches = _instagramUrlRegex.Matches(text);
        return matches.Select(t => t.Value).ToList();
    }

    private IEnumerable<string> GetDdInstagramUrlsFromMessage(string text)
    {
        var matches = _ddInstagramUrlRegex.Matches(text);
        return matches.Select(t => t.Value).ToList();
    }

    public Emote GetFixInstagramEmote()
    {
        return Emote.Parse($"<:{FixInstagramButtonEmojiName}:{FixInstagramButtonEmojiId}>");
    }
}