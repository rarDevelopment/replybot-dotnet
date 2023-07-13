namespace Replybot.TextCommands;

public class MessageToSend
{
    public Embed? Embed { get; set; }
    public IReadOnlyList<IEmote>? Reactions { get; set; }
    public bool StopProcessing { get; set; }
}