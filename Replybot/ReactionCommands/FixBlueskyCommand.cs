using System.Text.RegularExpressions;
using Replybot.Models;
using Replybot.Models.Bluesky;
using Replybot.ServiceLayer;

namespace Replybot.ReactionCommands;

public class FixBlueskyCommand : IReactionCommand
{
    private readonly BlueskyApi _blueskyApi;
    public readonly string NoLinkMessage = "I don't think there's a Bluesky link there.";
    private const string BlueskyUrlRegexPattern = "https?:\\/\\/(www.)?(bsky.app)\\/profile\\/[a-z0-9_.]+\\/post\\/[a-z0-9]+";
    private readonly Regex _blueskyUrlRegex = new(BlueskyUrlRegexPattern, RegexOptions.IgnoreCase);
    public const string FixTweetButtonEmojiId = "1126862392941367376";
    public const string FixTweetButtonEmojiName = "fixbluesky";

    public FixBlueskyCommand(BlueskyApi blueskyApi)
    {
        _blueskyApi = blueskyApi;
    }

    public bool CanHandle(string message, GuildConfiguration configuration)
    {
        return configuration.EnableFixBlueskyReactions && DoesMessageContainBlueskyUrl(message);
    }

    public Task<List<Emote>> HandleReaction(SocketMessage message)
    {
        var emotes = new List<Emote>
        {
            GetFixBlueskyEmote()
        };
        return Task.FromResult(emotes);
    }

    public bool IsReacting(IEmote reactionEmote, GuildConfiguration guildConfiguration)
    {
        return guildConfiguration.EnableFixBlueskyReactions && Equals(reactionEmote, GetFixBlueskyEmote());
    }

    public async Task<List<CommandResponse>> HandleMessage(IUserMessage message, IUser reactingUser)
    {
        if (!DoesMessageContainBlueskyUrl(message.Content))
        {
            return new List<CommandResponse>
            {
                new()
                {
                    Description = NoLinkMessage
                }
            };
        }

        var blueskyMessages = await GetBlueskyEmbeds(message);

        if (!blueskyMessages.Any())
        {
            return new List<CommandResponse>();
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
            fileAttachments = blueskyMessage.Images.Select(BuildFileAttachmentFromImage(fileDate)).ToList();
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

    private static Func<ImageWithMetadata, int, FileAttachment> BuildFileAttachmentFromImage(string fileDate)
    {
        return (image, index) =>
        {
            var fileName = $"bsky_{fileDate}_{index}.png";
            var fileAttachment = new FileAttachment(image.Image, fileName, image.AltText);
            return fileAttachment;
        };
    }

    private bool DoesMessageContainBlueskyUrl(string message)
    {
        return _blueskyUrlRegex.IsMatch(message);
    }

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

            var blueskyResponse = await _blueskyApi.GetRecord(repo, rkey);
            if (blueskyResponse == null)
            {
                continue;
            }

            var images = blueskyResponse.Value.Embed?.Media?.Images ?? blueskyResponse.Value.Embed?.Images;
            var imageCount = images?.Count ?? 0;

            var postText = blueskyResponse.Value.Text;

            var did = GetUserDidFromUri(blueskyResponse.Uri);
            if (did == null)
            {
                continue;
            }

            var imagesToSend = new List<ImageWithMetadata>();
            if (images != null && imageCount > 0)
            {
                foreach (var image in images)
                {
                    var blueskyImage = await _blueskyApi.GetImage(did, image.ImageData.Ref.Link);
                    if (blueskyImage != null)
                    {
                        imagesToSend.Add(new ImageWithMetadata(blueskyImage, image.Alt));
                    }
                }
            }

            blueskyEmbeds.Add(new BlueskyMessage
            {
                Title = $"@{repo}",
                Description = postText,
                Images = imagesToSend
            });
        }
        return blueskyEmbeds;
    }

    private static string? GetUserDidFromUri(string uri)
    {
        var uriSplit = uri.Replace("at://", "").Split("/");
        return uriSplit.FirstOrDefault(x => x.Contains("did:plc"));
    }

    private IEnumerable<string> GetBlueskyUrlsFromMessage(string text)
    {
        var matches = _blueskyUrlRegex.Matches(text);
        return matches.Select(t => t.Value).ToList();
    }

    private static Emote GetFixBlueskyEmote()
    {
        return Emote.Parse($"<:{FixTweetButtonEmojiName}:{FixTweetButtonEmojiId}>");
    }
}