using Replybot.Models;
using Replybot.TextCommands.Models;

namespace Replybot.ReactionCommands;

public class FixTikTokCommand(BotSettings botSettings, ApplicationEmojiSettings applicationEmojiSettings, DiscordSocketClient client)
    : FixUrlCommandBase(FixLinkConfig, botSettings, applicationEmojiSettings.FixTikTok, client), IReactionCommand
{
    public readonly string NoLinkMessage = "I don't think there's a TikTok link there.";
    private const string TikTokUrlRegexPattern = "https?:\\/\\/(vm.|www.)?(tiktok.com)/[\\@a-z0-9-_//]+";
    private const string VxTikTokUrlRegexPattern = "https?:\\/\\/(vm.|www.)?(vxtiktok.com)/[\\@a-z0-9-_//]+";
    private const string OriginalTikTokBaseUrl = "tiktok.com";
    private const string FixedTikTokBaseUrl = "vxtiktok.com";

    private static readonly FixLinkConfig FixLinkConfig = new(TikTokUrlRegexPattern,
        VxTikTokUrlRegexPattern,
        OriginalTikTokBaseUrl,
        FixedTikTokBaseUrl);

    public bool CanHandle(string message, GuildConfiguration configuration)
    {
        return configuration.EnableFixTikTokReactions &&
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
        return guildConfiguration.EnableFixTikTokReactions && Equals(reactionEmote, await GetEmote());
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