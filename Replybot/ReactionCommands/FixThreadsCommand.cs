using Replybot.Models;
using Replybot.TextCommands.Models;

namespace Replybot.ReactionCommands;

public class FixThreadsCommand(BotSettings botSettings, ApplicationEmojiSettings applicationEmojiSettings, DiscordSocketClient client)
    : FixUrlCommandBase(FixLinkConfig, botSettings, applicationEmojiSettings.FixThreads, client), IReactionCommand
{
    public readonly string NoLinkMessage = "I don't think there's a Threads link there.";
    private const string ThreadsUrlRegexPattern = @"https?:\/\/(www\.)?threads\.net\/(@[a-zA-Z0-9._]+\/post\/[a-zA-Z0-9_-]+)";
    private const string FixThreadsUrlRegexPattern = @"https?:\/\/(www\.)?fixthreads\.net\/(@[a-zA-Z0-9._]+\/post\/[a-zA-Z0-9_-]+)";
    private const string OriginalThreadsBaseUrl = "threads.net";
    private const string FixedThreadsBaseUrl = "fixthreads.net";

    private static readonly FixLinkConfig FixLinkConfig = new(ThreadsUrlRegexPattern,
        FixThreadsUrlRegexPattern,
        OriginalThreadsBaseUrl,
        FixedThreadsBaseUrl);

    public bool CanHandle(string message, GuildConfiguration configuration)
    {
        return configuration.EnableFixThreadsReactions &&
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
        return guildConfiguration.EnableFixThreadsReactions && Equals(reactionEmote, await GetEmote());
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