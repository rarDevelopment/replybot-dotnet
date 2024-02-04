using System.Text.RegularExpressions;
using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class GetEmoteCommand(BotSettings botSettings, IReplyBusinessLayer replyBusinessLayer,
        IDiscordFormatter discordFormatter)
    : ITextCommand
{
    private readonly string[] _triggers = { "emote", "emoji", "emojis" };
    private const string EmoteIdKey = "emoteId";
    private const string EmoteNameKey = "emoteName";
    private const string EmoteIdUrlKey = "emoteIdUrl";
    private const string DiscordEmoteRegexPattern = $@"<a?:(?<{EmoteNameKey}>[a-z0-9_]+):(?<{EmoteIdKey}>\d+)>";
    private readonly TimeSpan _matchTimeout = TimeSpan.FromMilliseconds(botSettings.RegexTimeoutTicks);
    private const string DiscordEmoteUrlTemplate =
        $"https://cdn.discordapp.com/emojis/{EmoteIdUrlKey}.png?size=128&quality=lossless";

    public bool CanHandle(TextCommandReplyCriteria replyCriteria)
    {
        return replyCriteria.IsBotNameMentioned &&
               _triggers.Any(t => replyBusinessLayer.GetWordMatch(t, replyCriteria.MessageText));
    }

    public async Task<CommandResponse> Handle(SocketMessage message)
    {
        var emoteMatches = Regex.Matches(message.Content, DiscordEmoteRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);

        if (emoteMatches.Count == 0)
        {
            if (message.Reference?.MessageId != null)
            {
                var repliedMessage = await message.Channel.GetMessageAsync(message.Reference.MessageId.Value);
                if (repliedMessage is { Content: not null })
                {
                    emoteMatches = Regex.Matches(repliedMessage.Content, DiscordEmoteRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
                }
            }
        }

        var emotes = new List<string>();

        if (emoteMatches.Count <= 0)
        {
            return new CommandResponse
            {
                Embed = discordFormatter.BuildErrorEmbedWithUserFooter("No emotes specified!",
                    "This command only works with custom Discord emotes (and does not include standard emojis)!",
                    message.Author),
                StopProcessing = true,
                NotifyWhenReplying = true,
            };
        }

        foreach (Match emoteMatch in emoteMatches)
        {
            if (!emoteMatch.Success || !emoteMatch.Groups.ContainsKey(EmoteIdKey) || !emoteMatch.Groups.ContainsKey(EmoteNameKey))
            {
                continue;
            }

            var emoteName = emoteMatch.Groups[EmoteNameKey].Value;
            var emoteId = emoteMatch.Groups[EmoteIdKey].Value;
            var emoteUrl = DiscordEmoteUrlTemplate.Replace(EmoteIdUrlKey, emoteId);
            emotes.Add($"{emoteName}: <{emoteUrl}>");
        }

        return new CommandResponse
        {
            Description = string.Join("\n", emotes),
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = true,
        };

    }

    private static string GetUserAvatarUrl(IUser user, string messageContent)
    {
        if (messageContent.ToLower().Contains("server"))
        {
            return (user as IGuildUser)?.GetDisplayAvatarUrl() ?? user.GetAvatarUrl(ImageFormat.Png);
        }

        return user.GetAvatarUrl(ImageFormat.Png);
    }
}