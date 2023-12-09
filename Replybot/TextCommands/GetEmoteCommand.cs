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
    private const string EmoteIdUrlKey = "emoteIdUrl";
    private readonly string _discordEmoteRegexPattern = $@"<a?:[a-z0-9_]+:(?<{EmoteIdKey}>\d{{18}})>";
    private readonly TimeSpan _matchTimeout = TimeSpan.FromMilliseconds(botSettings.RegexTimeoutTicks);
    private const string DiscordEmoteUrlTemplate =
        $"https://cdn.discordapp.com/emojis/{EmoteIdUrlKey}.png?size=96&quality=lossless";

    public bool CanHandle(TextCommandReplyCriteria replyCriteria)
    {
        return replyCriteria.IsBotNameMentioned &&
               _triggers.Any(t => replyBusinessLayer.GetWordMatch(t, replyCriteria.MessageText));
    }

    public Task<CommandResponse> Handle(SocketMessage message)
    {
        //if (message.Channel is not IGuildChannel { Guild: SocketGuild guild })
        //{
        //    return Task.FromResult(new CommandResponse
        //    {
        //        Embed = discordFormatter.BuildErrorEmbedWithUserFooter("Not a Server",
        //            "This command can only be used in a Discord server, it will not work in a DM.", message.Author),
        //        StopProcessing = true,
        //        NotifyWhenReplying = true,
        //    });
        //}

        var emoteMatches = Regex.Matches(message.Content, _discordEmoteRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);

        var emotes = new List<string>();

        if (emoteMatches.Count > 0)
        {
            foreach (Match emoteMatch in emoteMatches)
            {
                if (emoteMatch.Success && emoteMatch.Groups.ContainsKey(EmoteIdKey))
                {
                    var emoteId = emoteMatch.Groups[EmoteIdKey].Value;
                    var emoteUrl = DiscordEmoteUrlTemplate.Replace(EmoteIdUrlKey, emoteId);
                    emotes.Add($"<{emoteUrl}>");

                }
            }

            return Task.FromResult(new CommandResponse
            {
                Description = string.Join("\n", emotes),
                Reactions = null,
                StopProcessing = true,
                NotifyWhenReplying = true,
            });
        }

        else
        {
            return Task.FromResult(new CommandResponse
            {
                Embed = discordFormatter.BuildErrorEmbedWithUserFooter("No emotes specified!",
                    "This command only works with custom Discord emotes (and does not include standard emojis)!", message.Author),
                StopProcessing = true,
                NotifyWhenReplying = true,
            });
        }
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