using System.Text.RegularExpressions;
using Replybot.Models;
using static System.Text.RegularExpressions.Regex;

namespace Replybot.ReactionCommands;

public class FixInstagramCommand : IReactionCommand
{
    public readonly string NoLinkMessage = "I don't think there's an Instagram link there.";
    private const string InstagramUrlRegexPattern = "https?:\\/\\/(www.)?(instagram.com)\\/(p|reel|reels)\\/[a-z0-9-_]+";
    private const string DdInstagramUrlRegexPattern = "https?:\\/\\/(www.)?(ddinstagram.com)\\/(p|reel|reels)\\/[a-z0-9-_]+";
    private readonly TimeSpan _matchTimeout;
    public const string FixInstagramButtonEmojiId = "1116574189592260658";
    public const string FixInstagramButtonEmojiName = "fixinstagram";
    private const string OriginalInstagramBaseUrl = "instagram.com";
    private const string FixedInstagramBaseUrl = "ddinstagram.com";

    public FixInstagramCommand(BotSettings botSettings)
    {
        _matchTimeout = new TimeSpan(botSettings.RegexTimeoutTicks);
    }

    public bool CanHandle(string message, GuildConfiguration configuration)
    {
        return configuration.EnableFixInstagramReactions &&
               (DoesMessageContainInstagramUrl(message) || DoesMessageContainDdInstagramUrl(message));
    }

    public Task<List<Emote>> HandleReaction(SocketMessage message)
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

    public Task<List<CommandResponse>> HandleMessage(IUserMessage message, IUser reactingUser)
    {
        string? fixedMessage;
        if (DoesMessageContainInstagramUrl(message.Content))
        {
            fixedMessage = BuildFixedInstagramMessage(message, reactingUser, message.Author);
        }
        else if (DoesMessageContainDdInstagramUrl(message.Content))
        {
            fixedMessage = BuildOriginalInstagramMessage(message, reactingUser, message.Author);
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

    private string BuildFixedInstagramMessage(IMessage message, IUser requestingUser, IUser userWhoSent)
    {
        var fixedInstagramUrls = FixInstagramUrls(message);
        var describeText = fixedInstagramUrls.Count == 1 ? "post" : "posts";
        var isAre = fixedInstagramUrls.Count == 1 ? "is" : "are";
        var differentUserText = requestingUser.Id != userWhoSent.Id
            ? $" (in {userWhoSent.Username}'s message)"
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
            ? $" (in {userWhoSent.Username}'s message)"
            : "";
        var authorMentionMessage = $"{requestingUser.Mention} Here {isAre} the original {describeText}{differentUserText}: \n";
        return $"{authorMentionMessage}{string.Join("\n", fixedInstagramUrls)}";
    }

    private bool DoesMessageContainInstagramUrl(string message)
    {
        return IsMatch(message, InstagramUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
    }

    private bool DoesMessageContainDdInstagramUrl(string message)
    {
        return IsMatch(message, DdInstagramUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
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
        var matches = Matches(text, InstagramUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        return matches.Select(t => t.Value).ToList();
    }

    private IEnumerable<string> GetDdInstagramUrlsFromMessage(string text)
    {
        var matches = Matches(text, DdInstagramUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        return matches.Select(t => t.Value).ToList();
    }

    private static Emote GetFixInstagramEmote()
    {
        return Emote.Parse($"<:{FixInstagramButtonEmojiName}:{FixInstagramButtonEmojiId}>");
    }
}