using Replybot.Models;
using Replybot.TextCommands.Models;

namespace Replybot.ReactionCommands;

public class FixInstagramCommand(BotSettings botSettings, ApplicationEmojiSettings applicationEmojiSettings, DiscordSocketClient client)
    : FixUrlCommandBase(FixLinkConfig, botSettings, applicationEmojiSettings.FixInstagram, client), IReactionCommand
{
    public readonly string NoLinkMessage = "I don't think there's an Instagram link there.";
    private const string InstagramUrlRegexPattern = "https?:\\/\\/(www.)?(instagram.com)\\/(p|reel|reels)\\/[a-z0-9-_]+";
    private const string DdInstagramUrlRegexPattern = "https?:\\/\\/(www.)?(ddinstagram.com)\\/(p|reel|reels)\\/[a-z0-9-_]+";
    private const string OriginalInstagramBaseUrl = "instagram.com";
    private const string FixedInstagramBaseUrl = "ddinstagram.com";

    private static readonly FixLinkConfig FixLinkConfig = new(InstagramUrlRegexPattern,
        DdInstagramUrlRegexPattern,
        OriginalInstagramBaseUrl,
        FixedInstagramBaseUrl);

    public bool CanHandle(string message, GuildConfiguration configuration)
    {
        return configuration.EnableFixInstagramReactions &&
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
        return guildConfiguration.EnableFixInstagramReactions && Equals(reactionEmote, await GetEmote());
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