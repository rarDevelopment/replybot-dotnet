using Replybot.Models;
using Replybot.TextCommands.Models;

namespace Replybot.ReactionCommands;

public class FixTwitterCommand(BotSettings botSettings, ApplicationEmojiSettings applicationEmojiSettings, DiscordSocketClient client)
    : FixUrlCommandBase(FixLinkConfig, botSettings, applicationEmojiSettings.FixTwitter, client), IReactionCommand
{
    public readonly string NoLinkMessage = "I don't think there's a Twitter link there.";
    private const string TwitterUrlRegexPattern = $"https?:\\/\\/(www.)?(?<{MatchedDomainKey}>(twitter.com|t.co|x.com|nitter.net))\\/[a-z0-9_]+\\/status\\/[0-9]+";
    private const string VxTwitterUrlRegexPattern = $"https?:\\/\\/(www.)?(?<{MatchedDomainKey}>(vxtwitter.com|fxtwitter.com|fixvx.com))\\/[a-z0-9_]+\\/status\\/[0-9]+";
    private const string OriginalTwitterBaseUrl = "x.com";
    private const string FixedTwitterBaseUrl = "vxtwitter.com";
    private const string MatchedDomainKey = "matchedDomain";

    private static readonly FixLinkConfig FixLinkConfig = new(TwitterUrlRegexPattern,
        VxTwitterUrlRegexPattern,
        OriginalTwitterBaseUrl,
        FixedTwitterBaseUrl,
        MatchedDomainKey);

    public bool CanHandle(string message, GuildConfiguration configuration)
    {
        return configuration.EnableFixTweetReactions &&
               (DoesMessageContainOriginalUrl(message) || DoesMessageContainFixedUrl(message));
    }

    public async Task<List<Emote>> HandleReaction(SocketMessage message)
    {
        var emotes = new List<Emote>
        {
            await GetEmote()
        };
        return emotes;
    }

    public async Task<bool> IsReactingAsync(IEmote reactionEmote, GuildConfiguration guildConfiguration)
    {
        return guildConfiguration.EnableFixTweetReactions && Equals(reactionEmote, await GetEmote());
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
            new() { Description = fixedMessage, NotifyWhenReplying = false, AllowDeleteButton = true }
        };
        return Task.FromResult(messagesToSend);
    }
}