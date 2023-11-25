using Replybot.Models;
using Replybot.TextCommands.Models;

namespace Replybot.ReactionCommands;

public class FixTweetsWithoutAccountCommand(BotSettings botSettings) : FixUrlCommandBase(FixLinkConfig, botSettings), IReactionCommand
{
    public readonly string NoLinkMessage = "I don't think there's a relevant link there.";
    private const string MatchedDomainKey = "matchedDomain";
    private const string TwitterUrlRegexPattern = $"https?:\\/\\/(www.)?(?<{MatchedDomainKey}>(twitter.com|fxtwitter.com|vxtwitter.com|t.co|x.com|fixvx.com))\\/[a-z0-9_]+\\/status\\/[0-9]+";
    private const string NitterUrlRegexPattern = $"https?:\\/\\/(www.)?(<{MatchedDomainKey}>(nitter.net))\\/[a-z0-9_]+\\/status\\/[0-9]+";
    public const string FixTweetButtonEmojiId = "1133174470966784100";
    public const string ViewTweetsWithoutAccountButtonEmojiName = "view_tweets_without_account";
    private const string OriginalTwitterBaseUrl = "twitter.com";
    private const string NitterBaseUrl = "nitter.net";
    private const string AdditionalMessage = "This will let you access that tweet and its thread without an account.\nNote: it is intentional that there is no preview.";

    private static readonly FixLinkConfig FixLinkConfig = new(TwitterUrlRegexPattern,
        NitterUrlRegexPattern,
        FixTweetButtonEmojiId,
        ViewTweetsWithoutAccountButtonEmojiName,
        OriginalTwitterBaseUrl,
        NitterBaseUrl,
        MatchedDomainKey,
        AdditionalMessage);

    public bool CanHandle(string message, GuildConfiguration configuration)
    {
        return configuration.EnableFixTweetReactions &&
               (DoesMessageContainOriginalUrl(message) || DoesMessageContainFixedUrl(message));
    }

    public Task<List<Emote>> HandleReaction(SocketMessage message)
    {
        var emotes = new List<Emote>
        {
            GetEmote()
        };
        return Task.FromResult(emotes);
    }

    public bool IsReacting(IEmote reactionEmote, GuildConfiguration guildConfiguration)
    {
        return guildConfiguration.EnableFixTweetReactions && Equals(reactionEmote, GetEmote());
    }

    public Task<List<CommandResponse>> HandleMessage(IUserMessage message, IUser reactingUser)
    {
        string? fixedMessage;
        if (DoesMessageContainOriginalUrl(message.Content))
        {
            fixedMessage = BuildFixedUrlsMessage(message, reactingUser, message.Author);
        }
        else if (DoesMessageContainFixedUrl(message.Content))
        {
            fixedMessage = BuildOriginalUrlsMessage(message, reactingUser, message.Author);
        }
        else
        {
            fixedMessage = NoLinkMessage;
        }

        var messagesToSend = new List<CommandResponse>
        {
            new()
            {
                Description = fixedMessage,
                AllowDeleteButton = true,
                NotifyWhenReplying = false
            }
        };
        return Task.FromResult(messagesToSend);
    }
}
