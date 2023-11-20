namespace Replybot.Models.Bluesky;

public class ImageWithMetadata(Stream image, string altText)
{
    public Stream Image { get; set; } = image;
    public string AltText { get; set; } = altText;
}