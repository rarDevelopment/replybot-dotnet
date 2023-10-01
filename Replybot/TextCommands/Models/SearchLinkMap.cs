namespace Replybot.TextCommands.Models;

public class SearchLinkMap
{
    public SearchLinkMap(string triggerRegexPattern, string name, string url)
    {
        TriggerRegexPattern = triggerRegexPattern;
        Name = name;
        Url = url;
    }

    public string TriggerRegexPattern { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
}