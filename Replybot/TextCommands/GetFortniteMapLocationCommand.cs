using System.Drawing;
using System.Text.RegularExpressions;
using Replybot.Models;
using Replybot.ServiceLayer;
using Replybot.TextCommands.Models;
using Color = System.Drawing.Color;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

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
        image.Save(memoryStream, ImageFormat.Jpeg);

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

    private async Task<(string LocationName, Bitmap LocationImage)?> GetFortniteMapLocation()
    {
        var mapLocations = await fortniteApi.GetFortniteMapLocations();
        if (mapLocations == null)
        {
            return null;
        }

        using var httpClient = new HttpClient();

        await using var stream = await httpClient.GetStreamAsync(mapLocations.Images.POIs.AbsoluteUri);
        var bitmap = new Bitmap(stream);

        var random = new Random();

        using var g = Graphics.FromImage(bitmap);

        g.TranslateTransform(dx: 1028, dy: 1129);

        const float scaleFactor = 0.007f;
        g.ScaleTransform(scaleFactor, scaleFactor);

        g.RotateTransform(-90);

        var randomIndex = random.Next(mapLocations.POIs.Count);
        var randomLocation = mapLocations.POIs[randomIndex];

        var point = new PointF(randomLocation.Location.X, randomLocation.Location.Y);

        var x = point.X;
        var y = point.Y;

        using var pen = new Pen(Color.Black, 7000);

        const double lineLength = 25000f;

        g.DrawLine(pen, (int)(x - lineLength / 2f), (int)(y - lineLength / 2f), (int)(x + lineLength / 2f), (int)(y + lineLength / 2f));
        g.DrawLine(pen, (int)(x - lineLength / 2f), (int)(y + lineLength / 2f), (int)(x + lineLength / 2f), (int)(y - lineLength / 2f));

        return (randomLocation.Name, bitmap);
    }
}