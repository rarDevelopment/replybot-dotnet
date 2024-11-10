using System.Text.RegularExpressions;
using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class GetEmoteCommand(BotSettings botSettings, IReplyBusinessLayer replyBusinessLayer,
        IDiscordFormatter discordFormatter, RoleHelper roleHelper, ILogger<DiscordBot> logger)
    : ITextCommand
{
    private readonly string[] _triggers = ["emote", "emoji", "emojis", "emotes"];
    private readonly string[] _addEmoteTriggers = ["add emote", "add emoji", "add emojis", "add emotes"];
    private const string EmoteIdKey = "emoteId";
    private const string EmoteNameKey = "emoteName";
    private const string EmoteIdUrlKey = "emoteIdUrl";
    private const string FileExtensionKey = "{{FILE_EXTENSION}}";
    private const string DiscordEmoteRegexPattern = $@"<a?:(?<{EmoteNameKey}>[a-z0-9_]+):(?<{EmoteIdKey}>\d+)>";
    private const int MaxEmoteAddCount = 5;
    private readonly TimeSpan _matchTimeout = TimeSpan.FromMilliseconds(botSettings.RegexTimeoutTicks);
    private const string DiscordEmoteUrlTemplate =
        $"https://cdn.discordapp.com/emojis/{EmoteIdUrlKey}.{FileExtensionKey}?size=128&quality=lossless";

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

        var emoteMessages = new List<string>();

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

        var emoteCount = 0;
        var isAddingEmotes = _addEmoteTriggers.Any(t => message.Content.ToLower().Contains(t));
        foreach (Match emoteMatch in emoteMatches)
        {
            if (!emoteMatch.Success || !emoteMatch.Groups.ContainsKey(EmoteIdKey) || !emoteMatch.Groups.ContainsKey(EmoteNameKey))
            {
                continue;
            }

            var isAnimated = emoteMatch.Value.Contains("<a");
            var emoteName = emoteMatch.Groups[EmoteNameKey].Value;
            var emoteId = emoteMatch.Groups[EmoteIdKey].Value;
            var emoteUrl = DiscordEmoteUrlTemplate
                .Replace(EmoteIdUrlKey, emoteId)
                .Replace(FileExtensionKey, isAnimated ? "gif" : "png");

            var emoteMessageToSend = $"`{emoteName}` [Emote Image Link](<{emoteUrl}>)";

            if (isAddingEmotes)
            {
                if (emoteCount > MaxEmoteAddCount)
                {
                    emoteMessageToSend += $"\nThis emote was not added. You can only add up to {MaxEmoteAddCount} emotes at a time.\n";
                }
                else
                {
                    if (message is { Channel: IGuildChannel guildChannel, Author: IGuildUser guildUser })
                    {
                        if (await roleHelper.CanAdministrate(guildChannel.Guild, guildUser, [guildUser.GuildPermissions.ManageEmojisAndStickers]))
                        {
                            using var httpClient = new HttpClient();
                            var imageData = await httpClient.GetByteArrayAsync(emoteUrl);
                            using var ms = new MemoryStream(imageData);
                            try
                            {
                                var addedEmote = await guildChannel.Guild.CreateEmoteAsync(emoteName, new Image(ms));
                                emoteMessageToSend += $"\nThis emote has been added to this server: {addedEmote}\n";
                            }
                            catch (Exception ex)
                            {
                                logger.LogError($"Failed to save emote: {ex.Message}");
                                emoteMessageToSend += "\nThis emote failed to add. Make sure this bot has permission to Manage Expressions (like emotes and stickers).\n";
                            }
                        }
                        else
                        {
                            emoteMessageToSend += "\nThis emote failed to add. You do not have permission to manage emotes in this server.\n";
                        }
                    }
                    else
                    {
                        emoteMessageToSend += "\nThis emote failed to add. You can only add emotes in a server and only if you have permission.\n";
                    }
                }
            }

            emoteMessages.Add(emoteMessageToSend);
            emoteCount++;
        }

        return new CommandResponse
        {
            Description = string.Join("\n", emoteMessages),
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = true
        };
    }
}