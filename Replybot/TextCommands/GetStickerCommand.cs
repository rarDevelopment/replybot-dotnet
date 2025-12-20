using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.TextCommands.Models;
using System.Text.RegularExpressions;

namespace Replybot.TextCommands;

public class GetStickerCommand(
    IReplyBusinessLayer replyBusinessLayer,
        IDiscordFormatter discordFormatter, RoleHelper roleHelper, BotSettings botSettings, ILogger<DiscordBot> logger)
    : ITextCommand
{
    private readonly string[] _triggers = ["sticker"];
    private readonly string[] _addStickerTriggers = ["add sticker"];
    private const string StickerIdUrlKey = "stickerIdUrl";
    private const string FileExtensionKey = "{{FILE_EXTENSION}}";
    private const string DiscordStickerUrlTemplate =
        $"https://cdn.discordapp.com/stickers/{StickerIdUrlKey}.{FileExtensionKey}?size=128&quality=lossless";

    private const int MaxStickerNameLength = 30;
    private readonly string[] _validImageFileTypes = ["image/png", "image/jpg", "image/jpeg", "image/gif"];
    private readonly TimeSpan _matchTimeout = TimeSpan.FromMilliseconds(botSettings.RegexTimeoutTicks);

    public bool CanHandle(TextCommandReplyCriteria replyCriteria)
    {
        return replyCriteria.IsBotNameMentioned &&
               _triggers.Any(t => replyBusinessLayer.GetWordMatch(t, replyCriteria.MessageText));
    }

    public async Task<CommandResponse> Handle(SocketMessage message)
    {
        var guild = (message.Channel as IGuildChannel)?.Guild;

        var messageToUseForStickerContent = message;

        var sticker = messageToUseForStickerContent.Stickers.FirstOrDefault();

        var validImage = messageToUseForStickerContent.Attachments
            .FirstOrDefault(a => _validImageFileTypes.Contains(a.ContentType.ToLower()));

        if (sticker == null && validImage == null)
        {
            if (messageToUseForStickerContent.Reference?.MessageId == null
                || await messageToUseForStickerContent.Channel.GetMessageAsync(messageToUseForStickerContent.Reference.MessageId.Value) is not SocketMessage
                    repliedMessage)
            {
                return new CommandResponse
                {
                    Embed = discordFormatter.BuildErrorEmbedWithUserFooter("No stickers found!",
                        "There were no stickers or images found.",
                        messageToUseForStickerContent.Author),
                    StopProcessing = true,
                    NotifyWhenReplying = true,
                };
            }

            messageToUseForStickerContent = repliedMessage;

            if (messageToUseForStickerContent.Stickers.Count > 0)
            {
                sticker = messageToUseForStickerContent.Stickers.First();
            }
            else if (messageToUseForStickerContent.Attachments.Count > 0)
            {
                validImage = messageToUseForStickerContent.Attachments
                    .FirstOrDefault(a => _validImageFileTypes.Contains(a.ContentType.ToLower()));
                if (validImage == null)
                {
                    return new CommandResponse
                    {
                        Embed = discordFormatter.BuildErrorEmbedWithUserFooter("No stickers found!",
                            "There were no stickers or images found.",
                            messageToUseForStickerContent.Author),
                        StopProcessing = true,
                        NotifyWhenReplying = true,
                    };
                }
            }
        }

        var isAddingSticker = _addStickerTriggers.Any(t => message.Content.Contains(t, StringComparison.CurrentCultureIgnoreCase));
        var isStickerFromImage = validImage != null;
        var addingStickerTrigger = _addStickerTriggers.FirstOrDefault(t => message.Content.Contains(t, StringComparison.CurrentCultureIgnoreCase));

        if (isStickerFromImage)
        {
            var url = validImage!.Url;
            using var httpClient = new HttpClient();
            var imageData = await httpClient.GetByteArrayAsync(url);
            var stickerName = Path.GetFileNameWithoutExtension(validImage.Filename);
            var match = Regex.Match(message.Content, @$"{addingStickerTrigger}\s+(.+)", RegexOptions.IgnoreCase, _matchTimeout);
            if (match.Success)
            {
                var matchedText = match.Groups[1].Value.Trim();
                stickerName = matchedText.Trim();
            }

            if (stickerName.Length > MaxStickerNameLength)
            {
                stickerName = stickerName[..MaxStickerNameLength];
            }

            using var ms = new MemoryStream(imageData);
            var stickerMessageToSend = $"`{stickerName}` [Sticker Image Link](<{url}>)";
            try
            {
                var addedSticker = await guild!.CreateStickerAsync(stickerName, new Image(ms), [stickerName]);
                stickerMessageToSend += $"\nThis sticker has been added to this server: {addedSticker.Name}\n";
            }
            catch (Discord.Net.HttpException ex)
            {
                logger.LogError($"Failed to save sticker: {ex.Message}");
                if (ex.DiscordCode == DiscordErrorCode.FileUploadTooBig)
                {
                    stickerMessageToSend +=
                        "\nThis sticker failed to add. The file is too big.\n";
                }

                if (ex.DiscordCode == DiscordErrorCode.MaximumStickersReached)
                {
                    stickerMessageToSend +=
                        "\nThis sticker failed to add. You have reached the maximum number of stickers in this server.\n";
                }
                else
                {
                    stickerMessageToSend +=
                        "\nThis sticker failed to add. Make sure this bot has permission to Manage Expressions (like stickers and stickers).\n";
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to save sticker: {ex.Message}");
                stickerMessageToSend +=
                    "\nThis sticker failed to add. Make sure this bot has permission to Manage Expressions (like stickers and stickers).\n";
            }
            return new CommandResponse
            {
                Description = stickerMessageToSend,
                Reactions = null,
                StopProcessing = true,
                NotifyWhenReplying = true
            };
        }
        else
        {
            if (sticker == null)
            {
                return new CommandResponse
                {
                    Embed = discordFormatter.BuildErrorEmbedWithUserFooter("No sticker specified!",
                        "This command requires either a sticker or an image!",
                        messageToUseForStickerContent.Author),
                    StopProcessing = true,
                    NotifyWhenReplying = true,
                };
            }

            var isGif = sticker.Format is StickerFormatType.Gif;
            var stickerUrl = DiscordStickerUrlTemplate
                .Replace(StickerIdUrlKey, sticker.Id.ToString())
                .Replace(FileExtensionKey, isGif ? "gif" : "png");

            var stickerMessageToSend = $"`{sticker.Name}` [Sticker Image Link](<{stickerUrl}>)";

            if (!isAddingSticker)
            {
                return new CommandResponse
                {
                    Description = stickerMessageToSend,
                    Reactions = null,
                    StopProcessing = true,
                    NotifyWhenReplying = true
                };
            }

            if (messageToUseForStickerContent is { Channel: IGuildChannel guildChannel, Author: IGuildUser guildUser })
            {
                if (await roleHelper.CanAdministrate(guildChannel.Guild, guildUser,
                        [guildUser.GuildPermissions.ManageEmojisAndStickers]))
                {
                    using var httpClient = new HttpClient();
                    var imageData = await httpClient.GetByteArrayAsync(stickerUrl);
                    using var ms = new MemoryStream(imageData);
                    try
                    {
                        var addedSticker =
                            await guildChannel.Guild.CreateStickerAsync(sticker.Name, new Image(ms), [sticker.Name]);
                        stickerMessageToSend += $"\nThis sticker has been added to this server: {addedSticker.Name}\n";
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Failed to save sticker: {ex.Message}");
                        stickerMessageToSend +=
                            "\nThis sticker failed to add. Make sure this bot has permission to Manage Expressions (like stickers and stickers).\n";
                    }
                }
                else
                {
                    stickerMessageToSend +=
                        "\nThis sticker failed to add. You do not have permission to manage stickers in this server.\n";
                }
            }
            else
            {
                stickerMessageToSend +=
                    "\nThis sticker failed to add. You can only add stickers in a server and only if you have permission.\n";
            }

            return new CommandResponse
            {
                Description = stickerMessageToSend,
                Reactions = null,
                StopProcessing = true,
                NotifyWhenReplying = true
            };
        }
    }
}