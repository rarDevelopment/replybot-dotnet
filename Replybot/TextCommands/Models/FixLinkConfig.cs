namespace Replybot.TextCommands.Models;

public record FixLinkConfig(string OriginalUrlRegexPattern,
    string FixedUrlRegexPattern,
    string OriginalBaseUrl,
    string FixedBaseUrl,
    string? MatchedDomainKey = null,
    string? AdditionalMessage = null)
{
    public string OriginalUrlRegexPattern { get; } = OriginalUrlRegexPattern;
    public string FixedUrlRegexPattern { get; } = FixedUrlRegexPattern;
    public string OriginalBaseUrl { get; } = OriginalBaseUrl;
    public string FixedBaseUrl { get; } = FixedBaseUrl;
    public string? MatchedDomainKey { get; } = MatchedDomainKey;
    public string? AdditionalMessage { get; set; } = AdditionalMessage;
}