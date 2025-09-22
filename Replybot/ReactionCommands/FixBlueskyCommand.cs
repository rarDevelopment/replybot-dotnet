using Replybot.Models;
using Replybot.TextCommands.Models;

namespace Replybot.ReactionCommands;

public class FixBlueskyCommand(
    BotSettings botSettings,
    ApplicationEmojiSettings applicationEmojiSettings,
    DiscordSocketClient client)
    : FixUrlCommandBase(FixLinkConfig, botSettings, applicationEmojiSettings.FixBluesky, client), IReactionCommand
{
    private const string NoLinkMessage = "I don't think there's a Bluesky link there.";

    private const string BlueskyUrlRegexPattern = $"https?:\\/\\/(?<{MatchedDomainKey}>(bsky.app))\\/profile\\/[a-z0-9_.]+\\/post\\/[a-z0-9]+";

    private const string VxBlueskyUrlRegexPattern =
        $"https?:\\/\\/(?<{MatchedDomainKey}>(vxbsky.app|bskyx.app|bskx.app|fbsky.app))\\/profile\\/[a-z0-9_.]+\\/post\\/[a-z0-9]+";

    private const string OriginalBlueskyBaseUrl = "bsky.app";
    private const string FixedBlueskyBaseUrl = "bskx.app";
    private const string MatchedDomainKey = "matchedDomain";

    private static readonly FixLinkConfig FixLinkConfig = new(BlueskyUrlRegexPattern,
        VxBlueskyUrlRegexPattern,
        OriginalBlueskyBaseUrl,
        FixedBlueskyBaseUrl,
        MatchedDomainKey);

    public bool CanHandle(string message, GuildConfiguration configuration)
    {
        return configuration.EnableFixBlueskyReactions &&
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
        return guildConfiguration.EnableFixBlueskyReactions && Equals(reactionEmote, await GetEmote());
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