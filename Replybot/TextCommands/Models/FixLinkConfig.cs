namespace Replybot.TextCommands.Models;

public class FixLinkConfig(string originalUrlRegexPattern,
    string fixedUrlRegexPattern,
    string originalBaseUrl,
    string fixedBaseUrl,
    string? matchedDomainKey = null,
    string? additionalMessage = null)
{
    public string OriginalUrlRegexPattern { get; } = originalUrlRegexPattern;
    public string FixedUrlRegexPattern { get; } = fixedUrlRegexPattern;
    public string OriginalBaseUrl { get; } = originalBaseUrl;
    public string FixedBaseUrl { get; } = fixedBaseUrl;
    public string? MatchedDomainKey { get; } = matchedDomainKey;
    public string? AdditionalMessage { get; set; } = additionalMessage;
}