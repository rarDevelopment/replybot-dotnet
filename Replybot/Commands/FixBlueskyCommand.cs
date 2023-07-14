using System.Text.RegularExpressions;
using DiscordDotNetUtilities.Interfaces;
using Replybot.Models;
using Replybot.Models.Bluesky;
using Replybot.ServiceLayer;
using Embed = Discord.Embed;

namespace Replybot.Commands;

public class FixBlueskyCommand : IReactCommand
{
    private readonly BlueskyApi _blueskyApi;
    private readonly IDiscordFormatter _discordFormatter;
    public readonly string NoLinkMessage = "I don't think there's a Bluesky link there.";
    private const string BlueskyUrlRegexPattern = "https?:\\/\\/(www.)?(bsky.app)\\/profile\\/[a-z0-9_.]+\\/post\\/[a-z0-9]+";
    private readonly Regex _blueskyUrlRegex;
    public const string FixTweetButtonEmojiId = "1126862392941367376";
    public const string FixTweetButtonEmojiName = "fixbluesky";

    public FixBlueskyCommand(BlueskyApi blueskyApi, IDiscordFormatter discordFormatter)
    {
        _blueskyApi = blueskyApi;
        _discordFormatter = discordFormatter;
        _blueskyUrlRegex = new Regex(BlueskyUrlRegexPattern, RegexOptions.IgnoreCase);
    }

    public bool CanHandle(string message, GuildConfiguration configuration)
    {
        return configuration.EnableFixBlueskyReactions && DoesMessageContainBlueskyUrl(message);
    }

    public Task<List<Emote>> HandleReact(SocketMessage message)
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

    public async Task<List<MessageToSend>> HandleMessage(IUserMessage message)
    {
        if (!DoesMessageContainBlueskyUrl(message.Content))
        {
            return new List<MessageToSend>
            {
                new()
                {
                    Description = NoLinkMessage
                }
            };
        }

        var embeds = await GetBlueskyEmbeds(message);

        if (!embeds.Any())
        {
            return new List<MessageToSend>();
        }

        var listOfMessagesToSend = new List<MessageToSend>();

        foreach (var embedWithImages in embeds)
        {
            var fileAttachments = new List<FileAttachment>();
            if (embedWithImages.images != null && embedWithImages.images.Any())
            {
                var index = 0;
                var fileDate = DateTime.Now.ToShortDateString();
                foreach (var image in embedWithImages.images)
                {
                    var fileName = $"bsky_{fileDate}_{index}.png";
                    var fileAttachment = new FileAttachment(image.Image, fileName, image.AltText);
                    fileAttachments.Add(fileAttachment);
                    index++;
                }

            }

            var description = $"Okay, here's the content of that Bluesky post:\n>>> ### {embedWithImages.embed.Title}\n {embedWithImages.embed.Description}";
            listOfMessagesToSend.Add(new MessageToSend
            {
                Description = description,
                FileAttachments = fileAttachments
            });
        }

        return listOfMessagesToSend;

    }

    private bool DoesMessageContainBlueskyUrl(string message)
    {
        return _blueskyUrlRegex.IsMatch(message);
    }

    private async Task<List<(Embed embed, List<ImageWithMetadata>? images)>> GetBlueskyEmbeds(IMessage messageToFix)
    {
        var urlsToFix = GetBlueskyUrlsFromMessage(messageToFix.Content);
        var blueskyEmbeds = new List<(Embed, List<ImageWithMetadata>?)>();

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

            var imagesToEmbed = new List<ImageWithMetadata>();
            if (images != null && imageCount > 0)
            {
                foreach (var image in images)
                {
                    var blueskyImage = await _blueskyApi.GetImage(did, image.ImageData.Ref.Link);
                    if (blueskyImage != null)
                    {
                        imagesToEmbed.Add(new ImageWithMetadata(blueskyImage, image.Alt));
                    }
                }
            }

            var embed = _discordFormatter.BuildRegularEmbed($"@{repo}",
                postText,
                new EmbedFooterBuilder
                {
                    Text = "Posted on Bluesky"
                });
            blueskyEmbeds.Add((embed, imagesToEmbed));
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