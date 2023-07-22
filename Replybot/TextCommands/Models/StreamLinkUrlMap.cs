namespace Replybot.TextCommands.Models;

public class StreamLinkUrlMap
{
    public StreamLinkUrlMap(string flag, string name, string countryCode, string searchWord)
    {
        Flag = flag;
        Name = name;
        SearchWord = searchWord;
        CountryCode = countryCode;
    }

    public string Flag { get; set; }
    public string Name { get; set; }
    public string CountryCode { get; set; }
    public string SearchWord { get; set; }
}