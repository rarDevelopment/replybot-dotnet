using DiscordDotNetUtilities.Interfaces;
using Replybot.BusinessLayer;
using Replybot.TextCommands.Models;

namespace Replybot.TextCommands;

public class GetStickerCommand(
    IReplyBusinessLayer replyBusinessLayer,
        IDiscordFormatter discordFormatter, RoleHelper roleHelper, ILogger<DiscordBot> logger)
    : ITextCommand
{
    private readonly string[] _triggers = ["sticker"];
    private readonly string[] _addStickerTriggers = ["add sticker"];
    private const string StickerIdUrlKey = "stickerIdUrl";
    private const string FileExtensionKey = "{{FILE_EXTENSION}}";
    private const string DiscordStickerUrlTemplate =
        $"https://cdn.discordapp.com/stickers/{StickerIdUrlKey}.{FileExtensionKey}?size=128&quality=lossless";

    public bool CanHandle(TextCommandReplyCriteria replyCriteria)
    {
        return replyCriteria.IsBotNameMentioned &&
               _triggers.Any(t => replyBusinessLayer.GetWordMatch(t, replyCriteria.MessageText));
    }

    public async Task<CommandResponse> Handle(SocketMessage message)
    {
        IStickerItem? sticker = null;
        if (message.Reference?.MessageId != null)
        {
            var repliedMessage = await message.Channel.GetMessageAsync(message.Reference.MessageId.Value);
            if (repliedMessage.Stickers.Any())
            {
                sticker = repliedMessage.Stickers.First();
            }
        }

        if (sticker == null)
        {
            return new CommandResponse
            {
                Embed = discordFormatter.BuildErrorEmbedWithUserFooter("No stickers specified!",
                    "This command only works with stickers!",
                    message.Author),
                StopProcessing = true,
                NotifyWhenReplying = true,
            };
        }

        var isAddingSticker = _addStickerTriggers.Any(t => message.Content.ToLower().Contains(t));

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

        if (message is { Channel: IGuildChannel guildChannel, Author: IGuildUser guildUser })
        {
            if (await roleHelper.CanAdministrate(guildChannel.Guild, guildUser,
                    [guildUser.GuildPermissions.ManageEmojisAndStickers]))
            {
                using var httpClient = new HttpClient();
                var imageData = await httpClient.GetByteArrayAsync(stickerUrl);
                using var ms = new MemoryStream(imageData);
                try
                {
                    var addedSticker = await guildChannel.Guild.CreateStickerAsync(sticker.Name, new Image(ms), [sticker.Name]);
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