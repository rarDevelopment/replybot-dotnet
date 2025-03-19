using Replybot.Models.Bluesky;

namespace Replybot.ReactionCommands;

public class BlueskyMessage
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<MediaWithMetadata>? Images { get; set; }
    public MediaWithMetadata? Video { get; set; }
    public string? OriginalUrl { get; set; }
}