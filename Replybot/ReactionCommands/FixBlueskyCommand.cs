using System.Text.RegularExpressions;
using Replybot.Models;
using Replybot.Models.Bluesky;
using Replybot.ServiceLayer;
using static System.Text.RegularExpressions.Regex;

namespace Replybot.ReactionCommands;

public class FixBlueskyCommand(
    BotSettings botSettings,
    ApplicationEmojiSettings applicationEmojiSettings,
    BlueskyApi blueskyApi,
    DiscordSocketClient client) : IReactionCommand
{
    public readonly string NoLinkMessage = "I don't think there's a Bluesky link there.";
    private const string ContentUnavailableText = "[content unavailable]";
    private readonly TimeSpan _matchTimeout = new(botSettings.RegexTimeoutTicks);
    private const string BlueskyUrlRegexPattern = "https?:\\/\\/(www.)?(bsky.app)\\/profile\\/[a-z0-9_.]+\\/post\\/[a-z0-9]+";

    public bool CanHandle(string message, GuildConfiguration configuration)
    {
        return configuration.EnableFixBlueskyReactions && DoesMessageContainBlueskyUrl(message);
    }

    public async Task<List<Emote>> HandleReaction(SocketMessage message)
    {
        var emotes = new List<Emote>
        {
            await GetFixBlueskyEmote()
        };
        return emotes;
    }

    public async Task<bool> IsReactingAsync(IEmote reactionEmote, GuildConfiguration guildConfiguration)
    {
        return guildConfiguration.EnableFixBlueskyReactions && Equals(reactionEmote, await GetFixBlueskyEmote());
    }

    public async Task<List<CommandResponse>> HandleMessage(IUserMessage message, IUser reactingUser)
    {
        if (!DoesMessageContainBlueskyUrl(message.Content))
        {
            return
            [
                new CommandResponse
                {
                    Description = NoLinkMessage
                }
            ];
        }

        var blueskyMessages = await GetBlueskyEmbeds(message);

        if (!blueskyMessages.Any())
        {
            return [];
        }

        var commandResponses = blueskyMessages.Select(blueskyMessage => BuildCommandResponse(blueskyMessage, reactingUser)).ToList();

        return commandResponses;

    }

    private static CommandResponse BuildCommandResponse(BlueskyMessage blueskyMessage, IUser reactingUser)
    {
        var fileAttachments = new List<FileAttachment>();
        var fileDate = DateTime.Now.ToShortDateString();
        if (blueskyMessage.Images != null && blueskyMessage.Images.Any())
        {
            fileAttachments = blueskyMessage.Images.Select(BuildFileAttachmentFromMedia(fileDate, "png")).ToList();
        }
        else if (blueskyMessage.Video != null)
        {
            var func = BuildFileAttachmentFromMedia(fileDate, "mp4");
            fileAttachments = [func(blueskyMessage.Video, 0)];
        }

        var description =
            $"{reactingUser.Mention} Here's the Bluesky post content you requested:\n>>> ### {blueskyMessage.Title}\n {blueskyMessage.Description}";
        return new CommandResponse
        {
            Description = description,
            FileAttachments = fileAttachments,
            NotifyWhenReplying = false,
            AllowDeleteButton = true,
        };
    }

    private static Func<MediaWithMetadata, int, FileAttachment> BuildFileAttachmentFromMedia(string fileDate, string fileType)
    {
        return (image, index) =>
        {
            var fileName = $"bsky_{fileDate}_{index}.{fileType}";
            var fileAttachment = new FileAttachment(image.mediaStream, fileName, image.AltText);
            return fileAttachment;
        };
    }

    private bool DoesMessageContainBlueskyUrl(string message) => IsMatch(message, BlueskyUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);

    private async Task<List<BlueskyMessage>> GetBlueskyEmbeds(IMessage messageToFix)
    {
        var urlsToFix = GetBlueskyUrlsFromMessage(messageToFix.Content);
        var blueskyEmbeds = new List<BlueskyMessage>();

        foreach (var urlToFix in urlsToFix)
        {
            var strippedUrl = urlToFix.Replace("https://", "")
                .Replace("http://", "")
                .Replace("bsky.app/profile/", "");
            var splitUrl = strippedUrl.Split("/");
            if (splitUrl.Length < 3)
            {
                continue;
            }

            var repo = splitUrl[0];
            var rkey = splitUrl[2];

            var blueskyRecord = await blueskyApi.GetRecord(repo, rkey);
            if (blueskyRecord == null)
            {
                continue;
            }

            var images = blueskyRecord.Value.Embed?.Media?.Images ?? blueskyRecord.Value.Embed?.Images;
            var imageCount = images?.Count ?? 0;

            var video = blueskyRecord.Value.Embed?.Media?.Video ?? blueskyRecord.Value.Embed?.Video;

            var quotedRecord = blueskyRecord.Value.Embed?.Record ?? null;

            var postText = blueskyRecord.Value.Text;

            if (quotedRecord != null)
            {
                postText += "\n**Quoted Post:**\n";
                // ReSharper disable once ConstantConditionalAccessQualifier
                // ReSharper is still warning me here because quotedRecord.Uri shouldn't be able to be null, but it is!
                var quotedUserDid = quotedRecord?.Uri != null ? GetUserDidFromUri(quotedRecord.Uri) : null;
                var quotedRkey = quotedRecord?.Uri != null ? GetRkeyFromUri(quotedRecord.Uri) : null;
                if (quotedUserDid != null && quotedRkey != null)
                {
                    var quotedBlueskyRecord = await blueskyApi.GetRecord(quotedUserDid, quotedRkey);
                    if (quotedBlueskyRecord != null)
                    {
                        postText += quotedBlueskyRecord.Value.Text;
                    }
                    else
                    {
                        postText += ContentUnavailableText;
                    }
                }
                else
                {
                    postText += ContentUnavailableText;
                }
            }

            var did = GetUserDidFromUri(blueskyRecord.Uri);
            if (did == null)
            {
                continue;
            }

            var imagesToSend = new List<MediaWithMetadata>();
            if (images != null && imageCount > 0)
            {
                foreach (var image in images)
                {
                    var blueskyImage = await blueskyApi.GetImageOrVideo(did, image.ImageData.Ref.Link);
                    if (blueskyImage != null)
                    {
                        imagesToSend.Add(new MediaWithMetadata(blueskyImage, image.Alt));
                    }
                }
            }

            MediaWithMetadata? videoToSend = null;
            if (video != null)
            {
                var blueskyVideo = await blueskyApi.GetImageOrVideo(did, video.Ref.Link);
                if (blueskyVideo != null)
                {
                    videoToSend = new MediaWithMetadata(blueskyVideo, "test");
                }
            }

            blueskyEmbeds.Add(new BlueskyMessage
            {
                Title = $"@{repo}",
                Description = postText,
                Images = imagesToSend,
                Video = videoToSend
            });
        }
        return blueskyEmbeds;
    }

    private static string? GetRkeyFromUri(string uri)
    {
        var uriSplit = uri.Replace("at://", "").Split("/");
        return uriSplit.LastOrDefault();
    }

    private static string? GetUserDidFromUri(string uri)
    {
        var uriSplit = uri.Replace("at://", "").Split("/");
        return uriSplit.FirstOrDefault(x => x.Contains("did:plc"));
    }

    private IEnumerable<string> GetBlueskyUrlsFromMessage(string text)
    {
        var matches = Matches(text, BlueskyUrlRegexPattern, RegexOptions.IgnoreCase, _matchTimeout);
        return matches.Select(t => t.Value).ToList();
    }

    private async Task<Emote> GetFixBlueskyEmote()
    {
        return await client.GetApplicationEmoteAsync(Convert.ToUInt64(applicationEmojiSettings.FixBluesky));
    }
}