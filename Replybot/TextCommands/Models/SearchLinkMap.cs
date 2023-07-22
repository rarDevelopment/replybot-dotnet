namespace Replybot.TextCommands.Models;

public class SearchLinkMap
{
    public SearchLinkMap(List<string> triggers, string name, string url)
    {
        Triggers = triggers;
        Name = name;
        Url = url;
    }

    public List<string> Triggers { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
}