namespace Replybot.TextCommands.Models;

public class SearchAndCountryPair
{
    public string? SearchText { get; }
    public string? Country { get; }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(SearchText?.Trim()) && !string.IsNullOrEmpty(Country?.Trim());
    }

    public SearchAndCountryPair(string? searchText, string? country)
    {
        SearchText = searchText;
        Country = country;
    }
}