namespace Replybot.TextCommands.Models;

public class SearchLinkMap(string triggerRegexPattern, string name, string url)
{
    public string TriggerRegexPattern { get; set; } = triggerRegexPattern;
    public string Name { get; set; } = name;
    public string Url { get; set; } = url;
}