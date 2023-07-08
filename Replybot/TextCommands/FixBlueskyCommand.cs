using System.Text.RegularExpressions;
using DiscordDotNetUtilities.Interfaces;
using Replybot.Models.Bluesky;
using Replybot.ServiceLayer;
using Embed = Discord.Embed;
using Image = Discord.Image;

namespace Replybot.TextCommands;

public class FixBlueskyCommand
{
    private readonly BlueskyApi _blueskyApi;
    private readonly IDiscordFormatter _discordFormatter;
    public readonly string NoLinkMessage = "I don't think there's a Bluesky link there.";
    private const string BlueskyUrlRegexPattern = "https?:\\/\\/(www.)?(bsky.app)\\/profile\\/[a-z0-9_.]+\\/post\\/[a-z0-9]+";
    private readonly Regex _blueskyUrlRegex = new(BlueskyUrlRegexPattern, RegexOptions.IgnoreCase);
    public const string FixTweetButtonEmojiId = "1126862392941367376";
    public const string FixTweetButtonEmojiName = "fixbluesky";

    public FixBlueskyCommand(BlueskyApi blueskyApi, IDiscordFormatter discordFormatter)
    {
        _blueskyApi = blueskyApi;
        _discordFormatter = discordFormatter;
    }

    public async Task<List<(Embed embed, List<ImageWithMetadata>? images)>> GetFixedBlueskyMessage(IUserMessage messageToFix)
    {
        if (DoesMessageContainBlueskyUrl(messageToFix))
        {
            return await GetBlueskyEmbeds(messageToFix);
        }

        return new List<(Embed, List<ImageWithMetadata>?)>
        {
            (_discordFormatter.BuildRegularEmbed("No Link Found", NoLinkMessage, embedFooterBuilder: null), null)
        };
    }

    public bool DoesMessageContainBlueskyUrl(IMessage messageReferenced)
    {
        return _blueskyUrlRegex.IsMatch(messageReferenced.Content);
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
            blueskyEmbeds.Add((embed, imagesToEmbed ?? null));
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

    public Emote GetFixBlueskyEmote()
    {
        return Emote.Parse($"<:{FixTweetButtonEmojiName}:{FixTweetButtonEmojiId}>");
    }
}