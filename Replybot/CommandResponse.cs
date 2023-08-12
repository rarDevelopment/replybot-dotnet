namespace Replybot;

public class CommandResponse
{
    public string? Description { get; set; }
    public List<FileAttachment> FileAttachments { get; set; } = new();
    public Embed? Embed { get; set; }
    public IReadOnlyList<IEmote>? Reactions { get; set; }
    public bool StopProcessing { get; set; }
    public bool NotifyWhenReplying { get; set; }
    public bool AllowDeleteButton { get; set; }
}