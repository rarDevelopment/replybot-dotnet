using Replybot.Models;
using Replybot.TextCommands.Models;

namespace Replybot.ReactionCommands;

public class FixTikTokCommand(BotSettings botSettings) : FixUrlCommandBase(FixLinkConfig, botSettings), IReactionCommand
{
    public readonly string NoLinkMessage = "I don't think there's a TikTok link there.";
    private const string TikTokUrlRegexPattern = "https?:\\/\\/(vm.|www.)?(tiktok.com)/[\\@a-z0-9-_//]+";
    private const string VxTikTokUrlRegexPattern = "https?:\\/\\/(vm.|www.)?(vxtiktok.com)/[\\@a-z0-9-_//]+";
    public const string FixTikTokButtonEmojiId = "1177645488858742844";
    public const string FixTikTokButtonEmojiName = "fixtiktok";
    private const string OriginalTikTokBaseUrl = "tiktok.com";
    private const string FixedTikTokBaseUrl = "vxtiktok.com";

    private static readonly FixLinkConfig FixLinkConfig = new(TikTokUrlRegexPattern,
        VxTikTokUrlRegexPattern,
        FixTikTokButtonEmojiId,
        FixTikTokButtonEmojiName,
        OriginalTikTokBaseUrl,
        FixedTikTokBaseUrl);

    public bool CanHandle(string message, GuildConfiguration configuration)
    {
        return configuration.EnableFixTikTokReactions &&
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
        return guildConfiguration.EnableFixTikTokReactions && Equals(reactionEmote, GetEmote());
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