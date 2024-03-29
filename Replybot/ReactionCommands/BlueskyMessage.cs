﻿using Replybot.Models.Bluesky;

namespace Replybot.ReactionCommands;

public class BlueskyMessage
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<ImageWithMetadata>? Images { get; set; }
}