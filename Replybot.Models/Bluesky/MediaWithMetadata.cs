namespace Replybot.Models.Bluesky;

public class MediaWithMetadata(Stream mediaStream, string altText)
{
    public Stream mediaStream { get; set; } = mediaStream;
    public string AltText { get; set; } = altText;
}