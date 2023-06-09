using System.Text.RegularExpressions;
using Replybot.Models;

namespace Replybot.TextCommands;

public class FixInstagramCommand
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

    public async Task<(string fixedMessage, MessageReference messageToReplyTo)?> GetFixedInstagramMessage(
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
        IUser userWhoSentLinks,
        (string NoLinkMessage, MessageReference) noLinkFoundTuple, TriggerKeyword triggerKeyword)
    {
        return triggerKeyword switch
        {
            TriggerKeyword.FixInstagram => DoesMessageContainInstagramUrl(messageToFix)
                ? (BuildFixedInstagramMessage(messageToFix, requestingUser, userWhoSentLinks),
                    new MessageReference(messageToFix.Id))
                : noLinkFoundTuple,
            TriggerKeyword.BreakInstagram => DoesMessageContainDdInstagramUrl(messageToFix)
                ? (BuildOriginalInstagramMessage(messageToFix, requestingUser, userWhoSentLinks),
                    new MessageReference(messageToFix.Id))
                : noLinkFoundTuple,
            _ => (noLinkFoundTuple)
        };
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

    public bool DoesMessageContainInstagramUrl(IMessage messageReferenced)
    {
        return _instagramUrlRegex.IsMatch(messageReferenced.Content);
    }

    public bool DoesMessageContainDdInstagramUrl(IMessage messageReferenced)
    {
        return _ddInstagramUrlRegex.IsMatch(messageReferenced.Content);
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