﻿using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.TextCommands.Models;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Replybot.TextCommands;

public class EmoteCommand(BotSettings botSettings, IReplyBusinessLayer replyBusinessLayer,
        IDiscordFormatter discordFormatter, RoleHelper roleHelper, ILogger<DiscordBot> logger)
    : ITextCommand
{
    private readonly string[] _triggers = ["emote", "emoji", "emojis", "emotes"];
    private readonly string[] _addEmoteTriggers =
    [
        "add emote",
        "add emoji",
        "add emojis",
        "add emotes",
        "create emote",
        "create emoji",
        "create emojis",
        "create emotes"
    ];

    private readonly string[] _allEmoteTriggers = ["all emotes", "all emojis", "download emotes", "download emojis"];

    private const string EmoteIdKey = "emoteId";
    private const string EmoteNameKey = "emoteName";
    private const string EmoteIdUrlKey = "emoteIdUrl";
    private const string FileExtensionKey = "{{FILE_EXTENSION}}";
    private const string DiscordEmoteRegexPattern = $@"<a?:(?<{EmoteNameKey}>[a-z0-9_]+):(?<{EmoteIdKey}>\d+)>";
    private const int MaxEmoteAddCount = 5;
    private readonly TimeSpan _matchTimeout = TimeSpan.FromMilliseconds(botSettings.RegexTimeoutTicks);
    private const string DiscordEmoteUrlTemplate =
        $"https://cdn.discordapp.com/emojis/{EmoteIdUrlKey}.{FileExtensionKey}?size=128&quality=lossless";
    private readonly string[] _validImageFileTypes = ["image/png", "image/jpg", "image/jpeg", "image/gif"];

    public bool CanHandle(TextCommandReplyCriteria replyCriteria)
    {
        return replyCriteria.IsBotNameMentioned &&
               _triggers.Any(t => replyBusinessLayer.GetWordMatch(t, replyCriteria.MessageText));
    }

    public async Task<CommandResponse> Handle(SocketMessage message)
    {
        var allEmotesTrigger = _allEmoteTriggers.FirstOrDefault(t => message.Content.ToLower().Contains(t));
        var isGettingAllEmotes = allEmotesTrigger != null;
        var guild = (message.Channel as IGuildChannel)?.Guild;

        if (isGettingAllEmotes)
        {
            if (guild == null || message.Author is not IGuildUser guildUser)
            {
                return new CommandResponse
                {
                    Embed = discordFormatter.BuildErrorEmbedWithUserFooter("Not a server!",
                        "This command only works in a server.",
                        message.Author),
                    StopProcessing = true,
                    NotifyWhenReplying = true,
                };
            }
            var emotes = await guild.GetEmotesAsync();
            var canAdministrate = await roleHelper.CanAdministrate(guild, guildUser,
                [guildUser.GuildPermissions.ManageEmojisAndStickers]);
            var httpClient = new HttpClient();

            if (!canAdministrate)
            {
                return new CommandResponse
                {
                    Embed = discordFormatter.BuildErrorEmbedWithUserFooter("No permission!",
                        "This command only works with custom Discord emotes (and does not include standard emojis)! " +
                        "If you're trying to use an image, it has to be in your own message, not one you're replying to.",
                        message.Author),
                    StopProcessing = true,
                    NotifyWhenReplying = true,
                };
            }
            if (emotes != null)
            {
                var urls = emotes.Select(e => e.Url).ToList();

                var zipStream = new MemoryStream();
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    foreach (var url in urls)
                    {
                        var imageData = await httpClient.GetByteArrayAsync(url);
                        var fileName = Path.GetFileName(new Uri(url).AbsolutePath);
                        var entry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
                        await using var entryStream = entry.Open();
                        await entryStream.WriteAsync(imageData);
                    }
                }

                zipStream.Position = 0;

                var fileAttachment = new FileAttachment(zipStream, $"{guild.Name} emotes.zip", "application/zip");

                return new CommandResponse
                {
                    Description = "All server emotes are in this zip file.",
                    FileAttachments = [fileAttachment],
                    Reactions = null,
                    StopProcessing = true,
                    NotifyWhenReplying = true
                };
            }
        }

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

        var validImages = message.Attachments
            .Where(a => _validImageFileTypes.Contains(a.ContentType.ToLower()))
            .ToList();

        if (emoteMatches.Count <= 0 && !validImages.Any())
        {
            return new CommandResponse
            {
                Embed = discordFormatter.BuildErrorEmbedWithUserFooter("No emotes found!",
                    "There were no emotes or images found.",
                    message.Author),
                StopProcessing = true,
                NotifyWhenReplying = true,
            };
        }

        var emoteCount = 0;
        var addingEmotesTrigger = _addEmoteTriggers.FirstOrDefault(t => message.Content.ToLower().Contains(t));
        var isAddingEmotes = addingEmotesTrigger != null;
        var isEmoteFromImage = validImages.Any();

        if (isEmoteFromImage)
        {
            var image = validImages.First();

            var url = image.Url;
            using var httpClient = new HttpClient();
            var imageData = await httpClient.GetByteArrayAsync(url);
            var emoteName = CleanStringForEmoteName(image.Filename);
            var match = Regex.Match(message.Content, @$"{addingEmotesTrigger}\s+(.+)", RegexOptions.IgnoreCase, _matchTimeout);
            if (match.Success)
            {
                var matchedText = match.Groups[1].Value.Trim();
                emoteName = CleanStringForEmoteName(matchedText.Trim());
            }
            using var ms = new MemoryStream(imageData);
            try
            {
                var addedEmote = await guild?.CreateEmoteAsync(emoteName, new Image(ms))!;
                emoteMessages.Add($"`{emoteName}` [Emote Image Link](<{addedEmote.Url}>)\nThis emote has been added to this server: {addedEmote}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to save emote: {ex.Message}");
                if (ex.Message.Contains("BINARY_TYPE_MAX_SIZE"))
                {
                    emoteMessages.Add($"The file `{image.Filename}` was too large. File size cannot be larger than 2048kb.");
                }
                else if (ex.Message.Contains("Asset exceeds maximum size"))
                {
                    emoteMessages.Add($"The image `{image.Filename}` was too large. This isn't an issue with the file size, so try making the image itself smaller.");
                }
                else if (ex.Message.Contains("STRING_TYPE_REGEX"))
                {
                    emoteMessages.Add($"The name `{emoteName}` is invalid");
                }
                else if (ex.Message.Contains("BASE_TYPE_BAD_LENGTH"))
                {
                    emoteMessages.Add($"The name `{emoteName}` is too long. The emote name can be no longer than 32 characters.");
                }
                else if (ex.Message.Contains("Maximum number of emojis reached"))
                {
                    emoteMessages.Add("There are no emoji slots remaining in this server.");
                }
                else
                {
                    emoteMessages.Add($"`{image.Filename}` [Emote Image Link](<{url}>)\nThere are an error adding this emote. Try a smaller image maybe.");
                }
            }
        }
        else
        {
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
        }

        return new CommandResponse
        {
            Description = string.Join("\n", emoteMessages),
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = true
        };
    }

    private static string CleanStringForEmoteName(string textToClean)
    {
        if (textToClean.Contains(" "))
        {
            var splitWords = textToClean.Split(" ");
            splitWords = splitWords.Select(s => char.ToUpper(s[0]) + s[1..]).ToArray();
            textToClean = string.Join("", splitWords);
        }
        textToClean = Path.GetFileNameWithoutExtension(textToClean);
        textToClean = Regex.Replace(textToClean, "[^a-zA-Z0-9_]", "");
        return textToClean;
    }
}