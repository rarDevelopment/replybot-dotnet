namespace Replybot.Models.Bluesky;

public class ImageWithMetadata
{
    public ImageWithMetadata(Stream image, string altText)
    {
        Image = image;
        AltText = altText;
    }

    public Stream Image { get; set; }
    public string AltText { get; set; }
}