using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using Replybot.Models;
using Replybot.ServiceLayer;
using Replybot.TextCommands.Models;
using Color = System.Drawing.Color;

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
        var fortniteMapLocation = await GetFortniteMapLocation();
        return new CommandResponse
        {
            Description = fortniteMapLocation,
            Reactions = null,
            StopProcessing = true,
            NotifyWhenReplying = false
        };
    }

    private async Task<string?> GetFortniteMapLocation()
    {
        var mapLocations = await fortniteApi.GetFortniteMapLocations();
        if (mapLocations == null)
        {
            return null;
        }

        // Download the image
        using var httpClient = new HttpClient();

        await using var stream = await httpClient.GetStreamAsync(mapLocations.Images.Blank.AbsoluteUri);
        var bitmap = new Bitmap(stream);

        Debug.Assert(bitmap.Width == 2048);
        Debug.Assert(bitmap.Height == 2048);

        //const int worldRadius = 135000;
        var stringOutput = "";
        Color[] colors = [Color.Red, Color.Blue, Color.Black, Color.Yellow, Color.Purple];
        var random = new Random();

        using var g = Graphics.FromImage(bitmap);

        g.TranslateTransform(dx: 1024, dy: 1129);

        const float scaleFactor = 0.007f;
        g.ScaleTransform(scaleFactor, scaleFactor);

        g.RotateTransform(-90);

        //foreach (var color in colors)
        //{
        var namedLocations = mapLocations.POIs.Where(l => !l.Id.ToLower().Contains("unnamed")).ToList();
        var randomIndex = random.Next(namedLocations.Count);
        var randomLocation = namedLocations[randomIndex];

        //var x = (randomLocation.Location.Y + worldRadius) / (worldRadius * 2) * bitmap.Width;
        //var y = (1 - (randomLocation.Location.X + worldRadius) / (worldRadius * 2)) * bitmap.Height;
        float radius = DistanceFromOrigin(-6890f, -75716);
        float x = randomLocation.Location.X - radius;
        float y = randomLocation.Location.Y - radius;

        using var pen = new Pen(Color.Red, 7);

        const double xSize = 100f;
        g.DrawLine(pen, (int)(x - xSize / 2f), (int)(y - xSize / 2f), (int)(x + xSize / 2f), (int)(y + xSize / 2f));
        g.DrawLine(pen, (int)(x - xSize / 2f), (int)(y + xSize / 2f), (int)(x + xSize / 2f), (int)(y - xSize / 2f));

        using SolidBrush b1 = new SolidBrush(Color.FromArgb(alpha: 64, baseColor: Color.Gold));
        FillCircleAroundPXPoint(g, b1, x, y);

        //stringOutput +=
        //    $"{randomLocation.Name} || {color.Name} || original: ({randomLocation.Location.X} , {randomLocation.Location.Y}) || scaled: ({x} , {y})\n";
        //}
        // Save the modified image
        const string outputPath = @"C:\temp\" + "output.jpg";
        bitmap.Save(outputPath);
        Console.WriteLine($"Image saved to {outputPath}");
        Console.WriteLine(stringOutput);

        return "test";
    }

    private static float DistanceFromOrigin(float x, float y)
    {
        double hypotenuse = (x * x) + (y * y);
        var radius = (float)Math.Sqrt(hypotenuse); // i.e. a circle's radius
        return radius;
    }

    static void FillCircleAroundPXPoint(Graphics g, SolidBrush b, Single x, Single y)
    {
        Single radius = DistanceFromOrigin(x, y);

        Single topLeftX = x - radius;
        Single topLeftY = y - radius;

        g.FillEllipse(b, x: topLeftX, y: topLeftY, width: radius * 2, height: radius * 2);

        using (Pen p = new Pen(Color.FromArgb(alpha: 255, baseColor: b.Color)))
        {
            g.DrawEllipse(p, x: topLeftX, y: topLeftY, width: radius * 2, height: radius * 2);
        }

        const Int32 dotSize = 5000;

        using (SolidBrush sb = new SolidBrush(Color.FromArgb(alpha: 255, baseColor: b.Color)))
        {
            g.FillEllipse(sb, x: x - (dotSize / 2), y: y - (dotSize / 2), width: (dotSize / 2), height: (dotSize / 2));
        }
    }

}