using System.Drawing;
using System.Text.RegularExpressions;
using DiscordDotNetUtilities.Interfaces;
using ImageMagick;
using Replybot.Models;
using Replybot.ServiceLayer;
using Image = Discord.Image;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

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

    public async Task<List<(Embed embed, Image? image)>> GetFixedBlueskyMessage(
        IUserMessage requestingMessage,
        TriggerKeyword keyword)
    {
        //var requestingUser = requestingMessage.Author;
        //IUser userWhoSent = requestingMessage.Author;

        //var requesterMessageReference = new MessageReference(requestingMessage.Id);
        //var noLinkFoundTuple = (new List<Embed> { _discordFormatter.BuildRegularEmbed("No Link Found", NoLinkMessage, embedFooterBuilder: null) }, requestingMessage);

        //var fixedLinksResult = FixLinksIfFound(requestingMessage, requestingUser, userWhoSent, noLinkFoundTuple, keyword);
        //if (fixedLinksResult != noLinkFoundTuple)
        //{
        //    return fixedLinksResult;
        //}

        if (DoesMessageContainBlueskyUrl(requestingMessage))
        {
            return await GetBlueskyEmbeds(requestingMessage);
        }

        return new List<(Embed, Image?)>
        {
            (_discordFormatter.BuildRegularEmbed("No Link Found", NoLinkMessage, embedFooterBuilder: null), null)
        };

        //if (requestingMessage.Reference == null)
        //{
        //    return noLinkFoundTuple;
        //}

        //var messageReferenceId = requestingMessage.Reference.MessageId.GetValueOrDefault(default);
        //if (messageReferenceId == default)
        //{
        //    return noLinkFoundTuple;
        //}

        //var messageReferenced = await requestingMessage.Channel.GetMessageAsync(messageReferenceId);
        //return messageReferenced is null
        //    ? ("I couldn't read that message for some reason, sorry!", requesterMessageReference)
        //    : await FixLinksIfFound(messageReferenced, requestingUser, messageReferenced.Author, noLinkFoundTuple, keyword);
    }

    //private async Task<(List<Embed>, MessageReference)> FixLinksIfFound(IMessage messageToFix,
    //    IUser requestingUser,
    //    IUser userWhoSentTweets,
    //    (List<Embed> NoLinkMessage, MessageReference) noLinkFoundTuple, TriggerKeyword triggerKeyword)
    //{
    //    return triggerKeyword switch
    //    {
    //        TriggerKeyword.FixBluesky => DoesMessageContainBlueskyUrl(messageToFix)
    //            ? (await GetBlueskyEmbeds(messageToFix),
    //                new MessageReference(messageToFix.Id))
    //            : noLinkFoundTuple,
    //        _ => noLinkFoundTuple
    //    };
    //}

    private async Task<string> BuildFixedTweetsMessage(IMessage message, IUser requestingUser, IUser userWhoSentTweets)
    {
        var fixedTweets = await GetBlueskyEmbeds(message);
        var tweetDescribeText = fixedTweets.Count == 1 ? "tweet" : "tweets";
        var isAre = fixedTweets.Count == 1 ? "is" : "are";
        var differentUserText = requestingUser.Id != userWhoSentTweets.Id
            ? $" (in {userWhoSentTweets.Mention}'s message)"
            : "";
        var authorMentionMessage = $"{requestingUser.Mention} Here {isAre} the fixed {tweetDescribeText}{differentUserText}:\n";
        return $"{authorMentionMessage}{string.Join("\n", fixedTweets)}";
    }

    public bool DoesMessageContainBlueskyUrl(IMessage messageReferenced)
    {
        return _blueskyUrlRegex.IsMatch(messageReferenced.Content);
    }

    private async Task<List<(Embed embed, Image? image)>> GetBlueskyEmbeds(IMessage messageToFix)
    {
        var urlsToFix = GetBlueskyUrlsFromMessage(messageToFix.Content);
        var blueskyEmbeds = new List<(Embed, Image?)>();

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

            var images = blueskyResponse.Value.Embed?.Media?.Images;
            var imageCount = images?.Count ?? 0;

            var postText = blueskyResponse.Value.Text;

            var did = GetUserDidFromUri(blueskyResponse.Uri);
            if (did == null)
            {
                continue;
            }

            Image? imageForEmbed = null;
            if (images != null && imageCount > 0)
            {
                //if (imageCount > 1)
                //{
                //    using var imageCollection = new MagickImageCollection();
                //    foreach (var image in images)
                //    {
                //        var imageStream = await _blueskyApi.GetImage(did, image.ImageData.Ref.Link);
                //        if (imageStream == null)
                //        {
                //            continue;
                //        }

                //        imageCollection.Add(new MagickImage(imageStream));
                //    }

                //    IMagickImage<ushort>? mosaicImage = null;
                //    if (imageCollection.Count > 1)
                //    {
                //        mosaicImage = imageCollection.Mosaic();
                //    }

                //    if (mosaicImage != null)
                //    {
                //        var memoryStream = new MemoryStream(mosaicImage.ToByteArray());
                //        imageForEmbed = new Image(memoryStream);
                //    }
                //}
                //else
                //{
                var singleImageStream = await _blueskyApi.GetImage(did, images[0].ImageData.Ref.Link);
                if (singleImageStream != null)
                {
                    imageForEmbed = new Image(singleImageStream);
                }
                //}
            }

            var embed = _discordFormatter.BuildRegularEmbed($"@{repo}",
                postText,
                new EmbedFooterBuilder
                {
                    Text = "Posted on Bluesky"
                });
            blueskyEmbeds.Add((embed, imageForEmbed ?? null));
        }
        return blueskyEmbeds;

    }

    //private void CombineImages()
    //{
    //    var stream = new MemoryStream();
    //    using Bitmap result = new Bitmap(26 * 512, 13 * 512);
    //    for (int x = 0; x < 26; x++)
    //    {
    //        for (int y = 0; y < 13; y++)
    //        {
    //            using Graphics g = Graphics.FromImage(result);
    //            g.DrawImage(images[x, y], x * 512, y * 512);
    //        }
    //    }

    //    result.Save(stream, ImageFormat.Png);
    //}

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