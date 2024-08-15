using Replybot.Models;
using Replybot.TextCommands.Models;

namespace Replybot.ReactionCommands;

public class FixRedditCommand(BotSettings botSettings, ApplicationEmojiSettings applicationEmojiSettings, DiscordSocketClient client)
    : FixUrlCommandBase(FixLinkConfig, botSettings, applicationEmojiSettings.FixReddit, client), IReactionCommand
{
    public readonly string NoLinkMessage = "I don't think there's a Reddit link there.";
    private const string RedditUrlRegexPattern = @"https?:\/\/(www.)?(reddit.com)\/r\/[\\a-z0-9-_\/\/]+";
    private const string VxRedditUrlRegexPattern = @"https?:\/\/(www.)?(vxreddit.com)\/r\/[\\a-z0-9-_\/\/]+";
    private const string OriginalRedditBaseUrl = "reddit.com";
    private const string FixedRedditBaseUrl = "vxreddit.com";

    private static readonly FixLinkConfig FixLinkConfig = new(RedditUrlRegexPattern,
        VxRedditUrlRegexPattern,
        OriginalRedditBaseUrl,
        FixedRedditBaseUrl);

    public bool CanHandle(string message, GuildConfiguration configuration)
    {
        return configuration.EnableFixRedditReactions &&
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
        return guildConfiguration.EnableFixRedditReactions && Equals(reactionEmote, await GetEmote());
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