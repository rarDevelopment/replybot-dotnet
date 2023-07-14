using Replybot.Models.Bluesky;

namespace Replybot.TextCommands;

public class BlueskyMessage
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<ImageWithMetadata>? Images { get; set; }
}