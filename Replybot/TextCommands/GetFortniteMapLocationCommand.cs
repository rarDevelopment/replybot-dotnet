using System.Text.RegularExpressions;
using Fortnite_API.Objects.V1;
using Replybot.BusinessLayer;
using Replybot.Models;
using Replybot.ServiceLayer;
using Replybot.TextCommands.Models;
using SkiaSharp;

namespace Replybot.TextCommands;

public class GetFortniteMapLocationCommand(FortniteApi fortniteApi, BotSettings botSettings, IGuildConfigurationBusinessLayer guildConfigurationBusinessLayer)
    : ITextCommand
{
    private const string TriggerRegexPattern = "(where(( a|')re)? we droppin(g)?)|(whither shall we descend)|(wwd)";
    private readonly TimeSpan _matchTimeout = TimeSpan.FromMilliseconds(botSettings.RegexTimeoutTicks);

    // values for translating the X and Y coordinates of the map image for drawing an X on the location
    private const int MapTranslationX = 969;
    private const int MapTranslationY = 1020;
    private const float ScaleFactor = 0.00685f;

    public bool CanHandle(TextCommandReplyCriteria replyCriteria)
    {
        return replyCriteria.IsBotNameMentioned &&
               Regex.IsMatch(replyCriteria.MessageText,
                   TriggerRegexPattern,
                   RegexOptions.IgnoreCase,
                   _matchTimeout);
    }

    public async Task<CommandResponse> Handle(SocketMessage message)
    {
        var onlyNamedLocations = false;
        if (message.Channel is IGuildChannel guildChannel)
        {
            var config = await guildConfigurationBusinessLayer.GetGuildConfiguration(guildChannel.Guild);
            if (config != null)
            {
                onlyNamedLocations = config.FortniteMapOnlyNamedLocations;
            }
        }

        var fortniteMapLocationAndImage = await GetFortniteMapLocation(onlyNamedLocations);
        if (fortniteMapLocationAndImage == null)
        {
            return new CommandResponse
            {
                Description = "Failed to pick a location. Just go to Tilted Towers I guess.",
                StopProcessing = true,
                NotifyWhenReplying = false
            };
        }

        var location = fortniteMapLocationAndImage.Value.Location;
        var image = fortniteMapLocationAndImage.Value.LocationImage;

        var memoryStream = new MemoryStream();
        using (var skiaStream = new SKManagedWStream(memoryStream))
        {
            image.Encode(skiaStream, SKEncodedImageFormat.Jpeg, 100);
        }

        var locationName = location.Name;
        var imageDescription = $"{locationName} at {location.Location.X}, {location.Location.Y}, {location.Location.Z}";
        var fileAttachment = new FileAttachment(memoryStream, $"{locationName}.jpg", imageDescription);

        return new CommandResponse
        {
            Description = $"## {locationName}",
            FileAttachments = [fileAttachment],
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = false
        };
    }

    private async Task<(MapV1POI Location, SKBitmap LocationImage)?> GetFortniteMapLocation(bool onlyNamedLocations)
    {
        var mapLocations = await fortniteApi.GetFortniteMapLocations();
        if (mapLocations == null)
        {
            return null;
        }

        var locations = mapLocations.POIs;
        if (onlyNamedLocations)
        {
            locations = locations.Where(s => s.Name.Replace(" ", "").All(char.IsUpper)).ToList();
        }

        var random = new Random();
        var randomIndex = random.Next(locations.Count);
        var randomLocation = locations[randomIndex];

        var bitmap = await GetMapImage(mapLocations);

        MarkLocationWithX(bitmap, randomLocation);

        return (randomLocation, bitmap);
    }

    private static void MarkLocationWithX(SKBitmap bitmap, MapV1POI location)
    {
        using var canvas = new SKCanvas(bitmap);

        canvas.Translate(MapTranslationX, MapTranslationY);

        canvas.Scale(ScaleFactor, ScaleFactor);

        var point = new SKPoint(location.Location.X, location.Location.Y);
        var x = point.X;
        var y = point.Y;

        using var paint = new SKPaint();
        paint.IsStroke = true;

        const float xLineLength = 25000f;
        const float whiteXLineLength = xLineLength * 1.1f;

        paint.Color = SKColors.White;
        paint.StrokeWidth = 9000;
        canvas.DrawLine(x - whiteXLineLength / 2f, y - whiteXLineLength / 2f, x + whiteXLineLength / 2f, y + whiteXLineLength / 2f, paint);
        canvas.DrawLine(x - whiteXLineLength / 2f, y + whiteXLineLength / 2f, x + whiteXLineLength / 2f, y - whiteXLineLength / 2f, paint);

        paint.Color = SKColors.Black;
        paint.StrokeWidth = 7000; // Original stroke width for the black X
        canvas.DrawLine(x - xLineLength / 2f, y - xLineLength / 2f, x + xLineLength / 2f, y + xLineLength / 2f, paint);
        canvas.DrawLine(x - xLineLength / 2f, y + xLineLength / 2f, x + xLineLength / 2f, y - xLineLength / 2f, paint);
    }

    private static async Task<SKBitmap> GetMapImage(MapV1 mapLocations)
    {
        using var httpClient = new HttpClient();
        await using var stream = await httpClient.GetStreamAsync(mapLocations.Images.POIs.AbsoluteUri);
        await using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return SKBitmap.Decode(memoryStream);
    }
}