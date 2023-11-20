namespace Replybot.TextCommands.Models;

public class SearchAndCountryPair(string? searchText, string? country)
{
    public string? SearchText { get; } = searchText;
    public string? Country { get; } = country;

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(SearchText?.Trim()) && !string.IsNullOrEmpty(Country?.Trim());
    }
}