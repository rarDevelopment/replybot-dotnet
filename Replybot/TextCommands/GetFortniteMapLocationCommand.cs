using System.Text.RegularExpressions;
using Replybot.Models;
using Replybot.ServiceLayer;
using Replybot.TextCommands.Models;
using SkiaSharp;

namespace Replybot.TextCommands;

public class GetFortniteMapLocationCommand(FortniteApi fortniteApi, BotSettings botSettings)
    : ITextCommand
{
    private const string TriggerRegexPattern = "(where(( a|')re)? we droppin(g)?)|(whither shall we descend)|(wwd)";
    private readonly TimeSpan _matchTimeout = TimeSpan.FromMilliseconds(botSettings.RegexTimeoutTicks);

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
        var fortniteMapLocationAndImage = await GetFortniteMapLocation();
        if (fortniteMapLocationAndImage == null)
        {
            return new CommandResponse
            {
                Description = "Failed to grab a location.",
                StopProcessing = true,
                NotifyWhenReplying = false
            };
        }
        var image = fortniteMapLocationAndImage.Value.LocationImage;

        var memoryStream = new MemoryStream();
        using (var skiaStream = new SKManagedWStream(memoryStream))
        {
            image.Encode(skiaStream, SKEncodedImageFormat.Jpeg, 100);
        }

        var locationName = fortniteMapLocationAndImage.Value.LocationName;
        var fileAttachment = new FileAttachment(memoryStream, $"{locationName}.jpg", locationName);

        return new CommandResponse
        {
            Description = locationName,
            FileAttachments = [fileAttachment],
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = false
        };
    }

    private async Task<(string LocationName, SKBitmap LocationImage)?> GetFortniteMapLocation()
    {
        var mapLocations = await fortniteApi.GetFortniteMapLocations();
        if (mapLocations == null)
        {
            return null;
        }

        using var httpClient = new HttpClient();
        await using var stream = await httpClient.GetStreamAsync(mapLocations.Images.Blank.AbsoluteUri);
        var bitmap = SKBitmap.Decode(stream);

        var random = new Random();

        using var canvas = new SKCanvas(bitmap);
        canvas.Translate(1028, 1129);

        const float scaleFactor = 0.007f;
        canvas.Scale(scaleFactor, scaleFactor);

        canvas.RotateDegrees(-90);

        var randomIndex = random.Next(mapLocations.POIs.Count);
        var randomLocation = mapLocations.POIs[randomIndex];

        var point = new SKPoint(randomLocation.Location.X, randomLocation.Location.Y);

        var x = point.X;
        var y = point.Y;

        using var paint = new SKPaint
        {
            Color = SKColors.Aqua,
            StrokeWidth = 7000,
            IsStroke = true
        };

        const float lineLength = 25000f; // Adjust this value to control the length of the lines
        canvas.DrawLine(x - lineLength / 2f, y - lineLength / 2f, x + lineLength / 2f, y + lineLength / 2f, paint);
        canvas.DrawLine(x - lineLength / 2f, y + lineLength / 2f, x + lineLength / 2f, y - lineLength / 2f, paint);

        //write to desktop for testing
        //using var image = SKImage.FromBitmap(bitmap);
        //using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        //var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "output.jpg");
        //await using var fileStream = File.OpenWrite(filePath);
        //data.SaveTo(fileStream);

        return (randomLocation.Name, bitmap);
    }
}